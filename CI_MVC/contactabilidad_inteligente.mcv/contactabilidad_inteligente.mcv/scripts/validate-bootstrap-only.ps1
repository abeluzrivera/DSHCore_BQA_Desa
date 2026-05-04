#!/usr/bin/env pwsh

<#
.SYNOPSIS
Validates that the project uses Bootstrap instead of Tailwind CSS

.DESCRIPTION
Checks for any references to Tailwind CSS in the codebase and reports findings
#>

Write-Host "?? Scanning for Tailwind CSS references..." -ForegroundColor Cyan

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$results = @()

# Search in all relevant files
$filesToCheck = Get-ChildItem -Path $projectRoot -Include "*.cshtml", "*.css", "*.js", "*.csproj" -Recurse -ErrorAction SilentlyContinue

$tailwindPatterns = @(
    "tailwind",
    "tailwindcss",
    "@tailwind",
    "postcss",
    "tailwind.config",
    "from-\[",
    "to-\[",
    "flex-col",
    "grid-cols-",
    "md:",
    "lg:",
    "flex ",
    "justify-between"
)

foreach ($file in $filesToCheck) {
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    
    foreach ($pattern in $tailwindPatterns) {
        if ($content -match $pattern) {
            $results += @{
                File    = $file.FullName.Replace($projectRoot, "")
                Pattern = $pattern
                Type    = if ($file.Extension -eq ".cshtml") { "HTML/Razor" } else { $file.Extension }
            }
        }
    }
}

if ($results.Count -gt 0) {
    Write-Host "??  Found potential Tailwind CSS references:" -ForegroundColor Yellow
    $results | Group-Object -Property File | ForEach-Object {
        Write-Host "  ?? $($_.Name)" -ForegroundColor Yellow
        $_.Group | ForEach-Object {
            Write-Host "     - Pattern: $($_.Pattern)" -ForegroundColor Gray
        }
    }
    exit 1
} else {
    Write-Host "? No Tailwind CSS references found!" -ForegroundColor Green
    Write-Host "? Project is configured to use Bootstrap exclusively" -ForegroundColor Green
    exit 0
}
