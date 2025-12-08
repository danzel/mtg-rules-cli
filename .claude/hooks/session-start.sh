#!/bin/bash
# SessionStart hook: Configure proxy and install .NET 10 SDK for Claude Code web environment

# Only run in Claude Code remote environment
if [ "$CLAUDE_CODE_REMOTE" != "true" ]; then
  exit 0
fi

PROXY_URL="$GLOBAL_AGENT_HTTP_PROXY"

if [ -z "$PROXY_URL" ]; then
    echo "Error: GLOBAL_AGENT_HTTP_PROXY not set"
    exit 1
fi

# Configure apt-get proxy
sudo tee /etc/apt/apt.conf.d/proxy.conf > /dev/null << EOF
Acquire::http::Proxy "$PROXY_URL";
Acquire::https::Proxy "$PROXY_URL";
EOF
echo "Configured apt proxy"

# Configure NuGet proxy
mkdir -p ~/.nuget/NuGet
cat > ~/.nuget/NuGet/NuGet.Config << EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <config>
    <add key="http_proxy" value="$PROXY_URL" />
    <add key="https_proxy" value="$PROXY_URL" />
  </config>
</configuration>
EOF
echo "Configured NuGet proxy"

# Install .NET 10 SDK (stable/GA)
if [ ! -f ~/.dotnet/dotnet ]; then
    echo "Installing .NET 10 SDK..."
    curl -fsSL https://builds.dotnet.microsoft.com/dotnet/scripts/v1/dotnet-install.sh | bash -s -- --channel STS --install-dir ~/.dotnet
    echo "Installed: $(~/.dotnet/dotnet --version)"
else
    echo ".NET SDK already installed: $(~/.dotnet/dotnet --version)"
fi

# Add to PATH for current session
export DOTNET_ROOT=~/.dotnet
export PATH=$DOTNET_ROOT:$PATH

# Add to shell profile for persistence
if ! grep -q 'DOTNET_ROOT' ~/.bashrc 2>/dev/null; then
    echo '' >> ~/.bashrc
    echo '# .NET SDK' >> ~/.bashrc
    echo 'export DOTNET_ROOT=~/.dotnet' >> ~/.bashrc
    echo 'export PATH=$DOTNET_ROOT:$PATH' >> ~/.bashrc
fi

echo "Done. Proxy configured and .NET SDK ready."
