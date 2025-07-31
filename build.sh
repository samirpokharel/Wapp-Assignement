#!/bin/bash

# SimpleLMS Docker Build Script
# Usage: ./build.sh [dev|prod|clean]

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is running
check_docker() {
    if ! docker info > /dev/null 2>&1; then
        print_error "Docker is not running. Please start Docker and try again."
        exit 1
    fi
}

# Build development environment
build_dev() {
    print_status "Building development environment..."
    docker-compose --profile dev up --build -d
    print_status "Development environment is running at http://localhost:5000"
}

# Build production environment
build_prod() {
    print_status "Building production environment..."
    docker-compose -f docker-compose.prod.yml up --build -d
    print_status "Production environment is running at http://localhost:80"
}

# Clean up Docker resources
cleanup() {
    print_status "Cleaning up Docker resources..."
    docker-compose down --rmi all --volumes --remove-orphans
    docker system prune -f
    print_status "Cleanup completed"
}

# Show usage
show_usage() {
    echo "Usage: $0 [dev|prod|clean]"
    echo ""
    echo "Commands:"
    echo "  dev     Build and run development environment with hot reload"
    echo "  prod    Build and run production environment with nginx"
    echo "  clean   Clean up Docker resources"
    echo ""
    echo "Examples:"
    echo "  $0 dev    # Start development environment"
    echo "  $0 prod   # Start production environment"
    echo "  $0 clean  # Clean up resources"
}

# Main script logic
main() {
    check_docker

    case "${1:-}" in
        "dev")
            build_dev
            ;;
        "prod")
            build_prod
            ;;
        "clean")
            cleanup
            ;;
        "help"|"-h"|"--help")
            show_usage
            ;;
        "")
            print_warning "No command specified. Using 'dev' as default."
            build_dev
            ;;
        *)
            print_error "Unknown command: $1"
            show_usage
            exit 1
            ;;
    esac
}

# Run main function with all arguments
main "$@" 