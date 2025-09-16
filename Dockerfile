# Azure ML Workspace Automation Framework - Docker Image

FROM python:3.11-slim

# Set working directory
WORKDIR /app

# Install system dependencies
RUN apt-get update && apt-get install -y \
    wget \
    gnupg \
    ca-certificates \
    curl \
    git \
    && rm -rf /var/lib/apt/lists/*

# Install Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Copy requirements first for better caching
COPY requirements.txt .
COPY pyproject.toml .

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Install Playwright browsers
RUN playwright install --with-deps chromium firefox webkit

# Copy source code
COPY src/ src/
COPY tests/ tests/
COPY conftest.py .
COPY .env.example .env

# Create directories for test results
RUN mkdir -p test-results/{screenshots,videos,traces,logs,reports}

# Set environment variables
ENV PYTHONPATH=/app/src
ENV ARTIFACTS_PATH=/app/test-results
ENV HEADLESS=true
ENV CI=true

# Create non-root user for security
RUN useradd -m -u 1000 testuser && \
    chown -R testuser:testuser /app
USER testuser

# Default command
CMD ["python", "-m", "pytest", "--html=test-results/reports/pytest-report.html", "--self-contained-html"]

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import sys; sys.exit(0)"

# Labels
LABEL maintainer="Zencoder <support@zencoder.com>"
LABEL description="Azure ML Workspace Automation Framework"
LABEL version="1.0.0"