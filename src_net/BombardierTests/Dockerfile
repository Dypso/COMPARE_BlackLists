FROM alpine:latest

# Install bombardier
RUN apk add --no-cache curl && \
    curl -L -o bombardier https://github.com/codesenberg/bombardier/releases/download/v1.2.5/bombardier-linux-amd64 && \
    chmod +x bombardier && \
    mv bombardier /usr/local/bin/

# Install Node.js and npm
RUN apk add --no-cache nodejs npm

# Set working directory
WORKDIR /tests

# Copy test files
COPY . .
RUN chmod +x run_tests.sh

# Install Node.js dependencies
RUN npm install

ENTRYPOINT ["/bin/sh"]