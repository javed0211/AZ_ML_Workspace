# Multi-stage Dockerfile for Azure ML Test Automation Framework
# Optimized for Linux containers in Azure Pipelines

# Stage 1: Base image with all dependencies
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS base

# Install Node.js 18.x
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    gnupg \
    ca-certificates \
    && curl -fsSL https://deb.nodesource.com/setup_18.x | bash - \
    && apt-get install -y nodejs \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install Python 3.11 and pip
RUN apt-get update && apt-get install -y \
    python3.11 \
    python3-pip \
    python3.11-venv \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Create symbolic links for python
RUN ln -sf /usr/bin/python3.11 /usr/bin/python && \
    ln -sf /usr/bin/python3.11 /usr/bin/python3

# Install Azure CLI
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Install SSH client for remote connections
RUN apt-get update && apt-get install -y \
    openssh-client \
    rsync \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install Xvfb and GUI dependencies for VS Code Desktop testing
RUN apt-get update && apt-get install -y \
    xvfb \
    x11vnc \
    fluxbox \
    dbus-x11 \
    libgtk-3-0 \
    libgbm1 \
    libasound2 \
    libnotify4 \
    libsecret-1-0 \
    libxss1 \
    xdg-utils \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Install VS Code
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > packages.microsoft.gpg && \
    install -D -o root -g root -m 644 packages.microsoft.gpg /etc/apt/keyrings/packages.microsoft.gpg && \
    sh -c 'echo "deb [arch=amd64,arm64,armhf signed-by=/etc/apt/keyrings/packages.microsoft.gpg] https://packages.microsoft.com/repos/code stable main" > /etc/apt/sources.list.d/vscode.list' && \
    rm -f packages.microsoft.gpg && \
    apt-get update && \
    apt-get install -y code && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /workspace

# Stage 2: Install Playwright dependencies
FROM base AS playwright-deps

# Install Playwright system dependencies
RUN apt-get update && apt-get install -y \
    libnss3 \
    libnspr4 \
    libatk1.0-0 \
    libatk-bridge2.0-0 \
    libcups2 \
    libdrm2 \
    libdbus-1-3 \
    libxkbcommon0 \
    libxcomposite1 \
    libxdamage1 \
    libxfixes3 \
    libxrandr2 \
    libgbm1 \
    libpango-1.0-0 \
    libcairo2 \
    libasound2 \
    libatspi2.0-0 \
    libxshmfence1 \
    fonts-liberation \
    libappindicator3-1 \
    xdg-utils \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# Stage 3: Build and test stage
FROM playwright-deps AS build

# Copy project files
COPY . /workspace/

# Restore .NET dependencies
WORKDIR /workspace/NewFramework/CSharpTests
RUN dotnet restore

# Build the solution
RUN dotnet build --configuration Release --no-restore

# Install Node.js dependencies
WORKDIR /workspace/NewFramework/ElectronTests
RUN npm ci --production=false

# Install Playwright browsers (headless mode)
RUN npx playwright install chromium firefox --with-deps

# Install Python dependencies
WORKDIR /workspace/automation
RUN pip3 install --no-cache-dir -r requirements.txt

# Stage 4: Test execution stage
FROM build AS test

# Set environment variables for headless execution
ENV HEADLESS_MODE=true
ENV BROWSER_TYPE=chromium
ENV DISPLAY=:99
ENV PLAYWRIGHT_BROWSERS_PATH=/ms-playwright

# Configure Xvfb for GUI testing
ENV DISPLAY_WIDTH=1920
ENV DISPLAY_HEIGHT=1080
ENV DISPLAY_DEPTH=24

# Create Xvfb startup script
RUN echo '#!/bin/bash\n\
Xvfb :99 -screen 0 ${DISPLAY_WIDTH}x${DISPLAY_HEIGHT}x${DISPLAY_DEPTH} -ac +extension GLX +render -noreset &\n\
export DISPLAY=:99\n\
sleep 2\n\
exec "$@"' > /usr/local/bin/start-xvfb.sh && \
    chmod +x /usr/local/bin/start-xvfb.sh

# Create directories for test results
RUN mkdir -p /workspace/TestResults/BDD \
    /workspace/TestResults/UI \
    /workspace/TestResults/API \
    /workspace/TestResults/Coverage \
    /workspace/TestResults/Screenshots \
    /workspace/TestResults/Videos \
    /workspace/TestResults/Traces

# Set working directory
WORKDIR /workspace

# Default command runs all tests with Xvfb
ENTRYPOINT ["/usr/local/bin/start-xvfb.sh"]
CMD ["dotnet", "test", "NewFramework/CSharpTests/PlaywrightFramework.csproj", \
     "--configuration", "Release", \
     "--logger", "trx", \
     "--logger", "console;verbosity=detailed", \
     "--collect:XPlat Code Coverage", \
     "--results-directory", "/workspace/TestResults"]

# Stage 5: Production-ready minimal image (optional)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime

# Install only runtime dependencies
RUN apt-get update && apt-get install -y \
    curl \
    ca-certificates \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy built artifacts from build stage
COPY --from=build /workspace/NewFramework/CSharpTests/bin/Release /app/

# Entry point for production
ENTRYPOINT ["dotnet", "test"]