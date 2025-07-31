using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SimpleLMS.Data;
using SimpleLMS.Models;
using SimpleLMS.Models.ViewModels;

namespace SimpleLMS.Controllers;

[Authorize]
public class QuizController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public QuizController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Quiz/Create
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create(int contentItemId)
    {
        var contentItem = await _context.ContentItems
            .Include(ci => ci.Topic)
            .ThenInclude(t => t.Course)
            .FirstOrDefaultAsync(ci => ci.Id == contentItemId);

        if (contentItem == null)
        {
            return NotFound();
        }

        // Check if user is the course instructor or admin
        var userId = _userManager.GetUserId(User);
        var isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Admin");
        
        if (!isAdmin && contentItem.Topic.Course.Instructor != userId)
        {
            return Forbid();
        }

        ViewBag.ContentItem = contentItem;
        return View(new Quiz { ContentItemId = contentItemId });
    }

    // POST: Quiz/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Create([Bind("Title,Description,ContentItemId,TimeLimitMinutes,PassingScore,MaxAttempts")] Quiz quiz)
    {
        if (ModelState.IsValid)
        {
            // Check if quiz already exists for this content item
            var existingQuiz = await _context.Quizzes
                .FirstOrDefaultAsync(q => q.ContentItemId == quiz.ContentItemId);

            if (existingQuiz != null)
            {
                ModelState.AddModelError("", "A quiz already exists for this content item.");
                var contentItem = await _context.ContentItems
                    .Include(ci => ci.Topic)
                    .ThenInclude(t => t.Course)
                    .FirstOrDefaultAsync(ci => ci.Id == quiz.ContentItemId);
                ViewBag.ContentItem = contentItem;
                return View(quiz);
            }

            _context.Add(quiz);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Quiz created successfully!";
            return RedirectToAction("Edit", new { id = quiz.Id });
        }

        var contentItemForView = await _context.ContentItems
            .Include(ci => ci.Topic)
            .ThenInclude(t => t.Course)
            .FirstOrDefaultAsync(ci => ci.Id == quiz.ContentItemId);
        ViewBag.ContentItem = contentItemForView;
        return View(quiz);
    }

    // GET: Quiz/Edit/5
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var quiz = await _context.Quizzes
            .Include(q => q.ContentItem)
            .ThenInclude(ci => ci.Topic)
            .ThenInclude(t => t.Course)
            .Include(q => q.Questions.OrderBy(qq => qq.Order))
            .ThenInclude(qq => qq.Options.OrderBy(qo => qo.Order))
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        // Check if user is the course instructor or admin
        var userId = _userManager.GetUserId(User);
        var isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "Admin");
        
        if (!isAdmin && quiz.ContentItem.Topic.Course.Instructor != userId)
        {
            return Forbid();
        }

        return View(quiz);
    }

    // POST: Quiz/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,ContentItemId,TimeLimitMinutes,PassingScore,MaxAttempts,IsActive")] Quiz quiz)
    {
        if (id != quiz.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                quiz.UpdatedAt = DateTime.UtcNow;
                _context.Update(quiz);
                await _context.SaveChangesAsync();
                
                TempData["SuccessMessage"] = "Quiz updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(quiz.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction("Edit", new { id = quiz.Id });
        }

        var quizForView = await _context.Quizzes
            .Include(q => q.ContentItem)
            .ThenInclude(ci => ci.Topic)
            .ThenInclude(t => t.Course)
            .Include(q => q.Questions.OrderBy(qq => qq.Order))
            .ThenInclude(qq => qq.Options.OrderBy(qo => qo.Order))
            .FirstOrDefaultAsync(q => q.Id == id);
        return View(quizForView);
    }

    // GET: Quiz/Take/5
    public async Task<IActionResult> Take(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var quiz = await _context.Quizzes
            .Include(q => q.ContentItem)
            .ThenInclude(ci => ci.Topic)
            .ThenInclude(t => t.Course)
            .Include(q => q.Questions.OrderBy(qq => qq.Order))
            .ThenInclude(qq => qq.Options.OrderBy(qo => qo.Order))
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null || !quiz.IsActive)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        
        // Check if user is enrolled in the course
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == quiz.ContentItem.Topic.Course.Id);

        if (!isEnrolled)
        {
            return Forbid();
        }

        // Check if user has exceeded max attempts
        var attemptCount = await _context.QuizAttempts
            .CountAsync(qa => qa.UserId == userId && qa.QuizId == quiz.Id);

        if (attemptCount >= quiz.MaxAttempts)
        {
            TempData["ErrorMessage"] = "You have exceeded the maximum number of attempts for this quiz.";
            return RedirectToAction("Results", new { id = quiz.Id });
        }

        // Check if there's an active attempt
        var activeAttempt = await _context.QuizAttempts
            .Include(qa => qa.Answers)
            .FirstOrDefaultAsync(qa => qa.UserId == userId && qa.QuizId == quiz.Id && !qa.IsCompleted);

        if (activeAttempt != null)
        {
            // Check if time limit has expired
            var timeElapsed = DateTime.UtcNow - activeAttempt.StartedAt;
            if (timeElapsed.TotalMinutes > quiz.TimeLimitMinutes)
            {
                activeAttempt.IsCompleted = true;
                activeAttempt.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["ErrorMessage"] = "Time limit exceeded for your previous attempt.";
                return RedirectToAction("Results", new { id = quiz.Id });
            }

            return RedirectToAction("Continue", new { attemptId = activeAttempt.Id });
        }

        // Create new attempt
        var newAttempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quiz.Id,
            AttemptNumber = attemptCount + 1,
            TotalPoints = quiz.Questions.Sum(q => q.Points)
        };

        _context.QuizAttempts.Add(newAttempt);
        await _context.SaveChangesAsync();

        return RedirectToAction("Continue", new { attemptId = newAttempt.Id });
    }

    // GET: Quiz/Continue/5
    public async Task<IActionResult> Continue(int attemptId)
    {
        var attempt = await _context.QuizAttempts
            .Include(qa => qa.Quiz)
            .ThenInclude(q => q.Questions.OrderBy(qq => qq.Order))
            .ThenInclude(qq => qq.Options.OrderBy(qo => qo.Order))
            .Include(qa => qa.Answers)
            .FirstOrDefaultAsync(qa => qa.Id == attemptId);

        if (attempt == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (attempt.UserId != userId)
        {
            return Forbid();
        }

        // Check if time limit has expired
        var timeElapsed = DateTime.UtcNow - attempt.StartedAt;
        if (timeElapsed.TotalMinutes > attempt.Quiz.TimeLimitMinutes)
        {
            attempt.IsCompleted = true;
            attempt.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            TempData["ErrorMessage"] = "Time limit exceeded.";
            return RedirectToAction("Results", new { id = attempt.QuizId });
        }

        ViewBag.TimeRemaining = attempt.Quiz.TimeLimitMinutes - (int)timeElapsed.TotalMinutes;
        return View(attempt);
    }

    // POST: Quiz/Submit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int attemptId, Dictionary<int, object> answers)
    {
        var attempt = await _context.QuizAttempts
            .Include(qa => qa.Quiz)
            .ThenInclude(q => q.Questions)
            .ThenInclude(qq => qq.Options)
            .Include(qa => qa.Answers)
            .FirstOrDefaultAsync(qa => qa.Id == attemptId);

        if (attempt == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        if (attempt.UserId != userId)
        {
            return Forbid();
        }

        // Clear existing answers
        _context.QuizAttemptAnswers.RemoveRange(attempt.Answers);

        int totalScore = 0;

        foreach (var question in attempt.Quiz.Questions)
        {
            var answer = new QuizAttemptAnswer
            {
                QuizAttemptId = attemptId,
                QuizQuestionId = question.Id,
                PointsEarned = 0,
                IsCorrect = false
            };

            if (answers.ContainsKey(question.Id))
            {
                var answerValue = answers[question.Id];

                switch (question.QuestionType)
                {
                    case QuestionType.MultipleChoice:
                        if (int.TryParse(answerValue.ToString(), out int selectedOptionId))
                        {
                            var selectedOption = question.Options.FirstOrDefault(o => o.Id == selectedOptionId);
                            if (selectedOption != null)
                            {
                                answer.SelectedOptionId = selectedOptionId;
                                if (selectedOption.IsCorrect)
                                {
                                    answer.PointsEarned = question.Points;
                                    answer.IsCorrect = true;
                                    totalScore += question.Points;
                                }
                            }
                        }
                        break;

                    case QuestionType.TrueFalse:
                        if (bool.TryParse(answerValue.ToString(), out bool booleanAnswer))
                        {
                            answer.BooleanAnswer = booleanAnswer;
                            var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);
                            if (correctOption != null && correctOption.OptionText.ToLower() == booleanAnswer.ToString().ToLower())
                            {
                                answer.PointsEarned = question.Points;
                                answer.IsCorrect = true;
                                totalScore += question.Points;
                            }
                        }
                        break;

                    case QuestionType.ShortAnswer:
                    case QuestionType.Essay:
                        answer.AnswerText = answerValue.ToString();
                        // For text answers, manual grading would be needed
                        // For now, we'll mark as not correct
                        break;
                }
            }

            _context.QuizAttemptAnswers.Add(answer);
        }

        attempt.Score = totalScore;
        attempt.IsCompleted = true;
        attempt.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Quiz submitted successfully!";
        return RedirectToAction("Results", new { id = attempt.QuizId });
    }

    // GET: Quiz/Results/5
    public async Task<IActionResult> Results(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var quiz = await _context.Quizzes
            .Include(q => q.ContentItem)
            .ThenInclude(ci => ci.Topic)
            .ThenInclude(t => t.Course)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var userId = _userManager.GetUserId(User);
        
        // Check if user is enrolled in the course
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == quiz.ContentItem.Topic.Course.Id);

        if (!isEnrolled)
        {
            return Forbid();
        }

        var attempts = await _context.QuizAttempts
            .Include(qa => qa.Answers)
            .ThenInclude(qaa => qaa.QuizQuestion)
            .ThenInclude(qq => qq.Options)
            .Where(qa => qa.UserId == userId && qa.QuizId == quiz.Id)
            .OrderByDescending(qa => qa.CreatedAt)
            .ToListAsync();

        ViewBag.Quiz = quiz;
        ViewBag.Attempts = attempts;
        ViewBag.BestAttempt = attempts.OrderByDescending(a => a.PercentageScore).FirstOrDefault();

        return View();
    }

    private bool QuizExists(int id)
    {
        return _context.Quizzes.Any(e => e.Id == id);
    }
} 