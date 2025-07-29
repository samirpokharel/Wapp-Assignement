// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("mousemove", (e) => {
  if (Math.random() > 0.98) {
    createSparkle(e.clientX, e.clientY);
  }
});

function createSparkle(x, y) {
  const sparkle = document.createElement("div");
  sparkle.className =
    "fixed w-2 h-2 bg-white rounded-full pointer-events-none z-50";
  sparkle.style.left = x + "px";
  sparkle.style.top = y + "px";
  // NOTE: You need to define the 'ping' animation in CSS or Tailwind config for this to work
  sparkle.style.animation = "ping 1s cubic-bezier(0, 0, 0.2, 1)";
  document.body.appendChild(sparkle);

  setTimeout(() => {
    sparkle.remove();
  }, 1000);
}

// NOTE: The scroll-triggered animations are already handled by Tailwind classes,
// so this part of the script is no longer needed if you apply the classes directly in HTML.
