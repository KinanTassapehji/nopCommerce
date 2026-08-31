@echo off
set CLAUDE_CONFIG_DIR=%USERPROFILE%\.claude-kinan
claude --model claude-opus-5 --dangerously-skip-permissions --verbose %*