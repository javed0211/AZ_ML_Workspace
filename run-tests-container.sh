#!/bin/bash

###############################################################################
# Azure ML Test Automation - Container Test Runner
# 
# This script provides a convenient way to run tests in Docker containers
# Usage: ./run-tests-container.sh [options]
###############################################################################

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
TEST_CATEGORY="all"
BROWSER="chromium"
HEADLESS="true"
WORKERS="2"
BUILD_IMAGE="false"
USE_COMPOSE="false"
CLEANUP="false"
INTERACTIVE="false"

# Docker settings
IMAGE_NAME="azure-ml-tests"
IMAGE_TAG="latest"
CONTAINER_NAME="azure-ml-test-runner"

###############################################################################
# Functions
###############################################################################

print_header() {
    echo -e "${BLUE}================================================${NC}"
    echo -e "${BLUE}  Azure ML Test Automation - Container Runner${NC}"
    echo -e "${BLUE}================================================${NC}"
    echo ""
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ $1${NC}"
}

show_usage() {
    cat << EOF
Usage: $0 [OPTIONS]

Options:
    -c, --category <category>   Test category to run (all|bdd|ui|api|electron|python|vscode)
                                Default: all
    
    -b, --browser <browser>     Browser to use (chromium|firefox|webkit)
                                Default: chromium
    
    --headless <true|false>     Run in headless mode
                                Default: true
    
    -w, --workers <number>      Number of parallel workers
                                Default: 2
    
    --build                     Build Docker image before running tests
    
    --compose                   Use Docker Compose instead of Docker run
    
    --cleanup                   Clean up containers and images after tests
    
    -i, --interactive           Run container in interactive mode (for debugging)
    
    -h, --help                  Show this help message

Examples:
    # Run all tests with default settings
    $0

    # Run BDD tests only
    $0 --category bdd

    # Build image and run UI tests with Firefox
    $0 --build --category ui --browser firefox

    # Run tests using Docker Compose
    $0 --compose --category bdd

    # Interactive debugging
    $0 --interactive

    # Run with cleanup
    $0 --category all --cleanup

EOF
}

check_prerequisites() {
    print_info "Checking prerequisites..."
    
    # Check Docker
    if ! command -v docker &> /dev/null; then
        print_error "Docker is not installed. Please install Docker first."
        exit 1
    fi
    print_success "Docker is installed"
    
    # Check Docker daemon
    if ! docker info &> /dev/null; then
        print_error "Docker daemon is not running. Please start Docker."
        exit 1
    fi
    print_success "Docker daemon is running"
    
    # Check Docker Compose if needed
    if [ "$USE_COMPOSE" = "true" ]; then
        if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
            print_error "Docker Compose is not installed."
            exit 1
        fi
        print_success "Docker Compose is installed"
    fi
    
    echo ""
}

build_docker_image() {
    print_info "Building Docker image: ${IMAGE_NAME}:${IMAGE_TAG}"
    
    if docker build -t "${IMAGE_NAME}:${IMAGE_TAG}" .; then
        print_success "Docker image built successfully"
    else
        print_error "Failed to build Docker image"
        exit 1
    fi
    
    echo ""
}

run_tests_docker() {
    local category=$1
    local filter=""
    local results_dir="/workspace/TestResults"
    
    # Determine test filter
    case $category in
        bdd)
            filter="Category=BDD"
            results_dir="/workspace/TestResults/BDD"
            ;;
        ui)
            filter="Category=UI"
            results_dir="/workspace/TestResults/UI"
            ;;
        api)
            filter="Category=API"
            results_dir="/workspace/TestResults/API"
            ;;
        vscode)
            filter="Category=VSCode"
            results_dir="/workspace/TestResults/VSCode"
            ;;
        all)
            filter=""
            results_dir="/workspace/TestResults"
            ;;
        *)
            print_error "Unknown test category: $category"
            exit 1
            ;;
    esac
    
    print_info "Running ${category} tests in Docker container..."
    
    # Prepare Docker run command
    local docker_cmd="docker run --rm"
    
    # Add interactive flag if needed
    if [ "$INTERACTIVE" = "true" ]; then
        docker_cmd="$docker_cmd -it"
    fi
    
    # Add volume mounts
    docker_cmd="$docker_cmd -v $(pwd)/TestResults:/workspace/TestResults"
    
    # Add environment variables
    docker_cmd="$docker_cmd -e HEADLESS_MODE=${HEADLESS}"
    docker_cmd="$docker_cmd -e BROWSER_TYPE=${BROWSER}"
    docker_cmd="$docker_cmd -e WORKERS=${WORKERS}"
    
    # Add Azure credentials if available
    if [ -f .env ]; then
        docker_cmd="$docker_cmd --env-file .env"
    fi
    
    # Add container name
    docker_cmd="$docker_cmd --name ${CONTAINER_NAME}"
    
    # Add image
    docker_cmd="$docker_cmd ${IMAGE_NAME}:${IMAGE_TAG}"
    
    # Add test command
    if [ "$INTERACTIVE" = "true" ]; then
        docker_cmd="$docker_cmd /bin/bash"
    elif [ -n "$filter" ]; then
        docker_cmd="$docker_cmd dotnet test NewFramework/CSharpTests/PlaywrightFramework.csproj"
        docker_cmd="$docker_cmd --configuration Release"
        docker_cmd="$docker_cmd --logger trx"
        docker_cmd="$docker_cmd --logger 'console;verbosity=detailed'"
        docker_cmd="$docker_cmd --collect:'XPlat Code Coverage'"
        docker_cmd="$docker_cmd --filter '${filter}'"
        docker_cmd="$docker_cmd --results-directory ${results_dir}"
    fi
    
    # Execute Docker command
    if eval $docker_cmd; then
        print_success "Tests completed successfully"
        return 0
    else
        print_error "Tests failed"
        return 1
    fi
}

run_tests_compose() {
    local category=$1
    local service_name=""
    
    # Determine service name
    case $category in
        bdd)
            service_name="bdd-tests"
            ;;
        ui)
            service_name="ui-tests"
            ;;
        api)
            service_name="api-tests"
            ;;
        electron)
            service_name="electron-tests"
            ;;
        python)
            service_name="python-automation"
            ;;
        vscode)
            service_name="vscode-tests"
            ;;
        all)
            service_name=""
            ;;
        *)
            print_error "Unknown test category: $category"
            exit 1
            ;;
    esac
    
    print_info "Running tests using Docker Compose..."
    
    # Check if docker-compose.yml exists
    if [ ! -f docker-compose.yml ]; then
        print_error "docker-compose.yml not found"
        exit 1
    fi
    
    # Run Docker Compose
    if [ -z "$service_name" ]; then
        # Run all services
        if docker-compose up --abort-on-container-exit; then
            print_success "All tests completed successfully"
            return 0
        else
            print_error "Some tests failed"
            return 1
        fi
    else
        # Run specific service
        if docker-compose up --abort-on-container-exit "$service_name"; then
            print_success "${category} tests completed successfully"
            return 0
        else
            print_error "${category} tests failed"
            return 1
        fi
    fi
}

cleanup_resources() {
    print_info "Cleaning up Docker resources..."
    
    # Stop and remove containers
    if docker ps -a | grep -q "${CONTAINER_NAME}"; then
        docker rm -f "${CONTAINER_NAME}" 2>/dev/null || true
        print_success "Removed container: ${CONTAINER_NAME}"
    fi
    
    # Clean up Docker Compose resources
    if [ "$USE_COMPOSE" = "true" ]; then
        docker-compose down -v 2>/dev/null || true
        print_success "Cleaned up Docker Compose resources"
    fi
    
    # Optional: Remove images
    read -p "Do you want to remove Docker images? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        docker rmi "${IMAGE_NAME}:${IMAGE_TAG}" 2>/dev/null || true
        print_success "Removed Docker image"
    fi
    
    # Clean up dangling images and volumes
    docker system prune -f
    print_success "Cleaned up dangling resources"
    
    echo ""
}

show_test_results() {
    print_info "Test Results Summary:"
    echo ""
    
    if [ -d "TestResults" ]; then
        # Count test result files
        local trx_files=$(find TestResults -name "*.trx" 2>/dev/null | wc -l)
        local coverage_files=$(find TestResults -name "coverage.cobertura.xml" 2>/dev/null | wc -l)
        
        echo "  Test Result Files: ${trx_files}"
        echo "  Coverage Files: ${coverage_files}"
        echo ""
        echo "  Results Location: $(pwd)/TestResults"
        echo ""
        
        # Show directory structure
        if command -v tree &> /dev/null; then
            tree -L 2 TestResults
        else
            ls -la TestResults/
        fi
    else
        print_warning "No test results found"
    fi
    
    echo ""
}

###############################################################################
# Main Script
###############################################################################

main() {
    print_header
    
    # Parse command line arguments
    while [[ $# -gt 0 ]]; do
        case $1 in
            -c|--category)
                TEST_CATEGORY="$2"
                shift 2
                ;;
            -b|--browser)
                BROWSER="$2"
                shift 2
                ;;
            --headless)
                HEADLESS="$2"
                shift 2
                ;;
            -w|--workers)
                WORKERS="$2"
                shift 2
                ;;
            --build)
                BUILD_IMAGE="true"
                shift
                ;;
            --compose)
                USE_COMPOSE="true"
                shift
                ;;
            --cleanup)
                CLEANUP="true"
                shift
                ;;
            -i|--interactive)
                INTERACTIVE="true"
                shift
                ;;
            -h|--help)
                show_usage
                exit 0
                ;;
            *)
                print_error "Unknown option: $1"
                show_usage
                exit 1
                ;;
        esac
    done
    
    # Show configuration
    print_info "Configuration:"
    echo "  Test Category: ${TEST_CATEGORY}"
    echo "  Browser: ${BROWSER}"
    echo "  Headless Mode: ${HEADLESS}"
    echo "  Workers: ${WORKERS}"
    echo "  Build Image: ${BUILD_IMAGE}"
    echo "  Use Compose: ${USE_COMPOSE}"
    echo "  Interactive: ${INTERACTIVE}"
    echo ""
    
    # Check prerequisites
    check_prerequisites
    
    # Build image if requested
    if [ "$BUILD_IMAGE" = "true" ]; then
        build_docker_image
    fi
    
    # Create TestResults directory
    mkdir -p TestResults
    
    # Run tests
    local test_result=0
    if [ "$USE_COMPOSE" = "true" ]; then
        run_tests_compose "$TEST_CATEGORY"
        test_result=$?
    else
        run_tests_docker "$TEST_CATEGORY"
        test_result=$?
    fi
    
    # Show results
    if [ "$INTERACTIVE" = "false" ]; then
        show_test_results
    fi
    
    # Cleanup if requested
    if [ "$CLEANUP" = "true" ]; then
        cleanup_resources
    fi
    
    # Exit with test result
    if [ $test_result -eq 0 ]; then
        print_success "All operations completed successfully!"
        exit 0
    else
        print_error "Tests failed. Check the logs above for details."
        exit 1
    fi
}

# Run main function
main "$@"