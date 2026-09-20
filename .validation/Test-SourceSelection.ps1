$ErrorActionPreference = 'Stop'
$projectPath = Join-Path $PSScriptRoot 'HardcoreDesert.Validation.csproj'
$evaluatedJson = & dotnet msbuild $projectPath '-getItem:Compile,Page,ProjectReference'
if ($LASTEXITCODE -ne 0) { throw 'Project evaluation failed.' }
$evaluated = ($evaluatedJson -join "`n") | ConvertFrom-Json
$selected = @($evaluated.Items.Compile) + @($evaluated.Items.Page)
if (@($evaluated.Items.ProjectReference).Count) {
    throw 'Validation must not reference separately compiled Core/mod projects.'
}
$badLinks = @($selected | Where-Object {
    -not $_.Link -or
    [System.IO.Path]::IsPathRooted($_.Link) -or
    $_.Link -match '([\\/]$|(^|[\\/])\.\.([\\/]|$))'
})
$duplicateLinks = @($selected | Group-Object Link | Where-Object Count -gt 1)
if ($badLinks.Count -or $duplicateLinks.Count) {
    throw "Invalid links: $($badLinks.Count); duplicate links: $($duplicateLinks.Count)."
}

# Build the expected overlay independently of the MSBuild item expressions.
$expected = @{}
$coreDirectory = (Resolve-Path (Join-Path $PSScriptRoot '../../Core.cpk')).Path
$modDirectory = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
foreach ($sourceDirectory in @($coreDirectory, $modDirectory)) {
    foreach ($sourceFile in Get-ChildItem -LiteralPath $sourceDirectory -Recurse -File) {
        if ($sourceFile.Extension -notin @('.cs', '.xaml')) { continue }
        $relativePath = $sourceFile.FullName.Substring($sourceDirectory.Length + 1)
        if ($relativePath -match '(^|\\)(\.[^\\]*|bin|obj|packages|LibsGameApi)(\\|$)') { continue }
        $expected[$relativePath] = $sourceFile.FullName
    }
}
foreach ($sourceItem in $selected) {
    if (-not $expected.ContainsKey($sourceItem.Link) -or
        $expected[$sourceItem.Link] -ne $sourceItem.FullPath) {
        throw "Incorrect source selection: $($sourceItem.Link) -> $($sourceItem.FullPath)"
    }
}
if ($expected.Count -ne $selected.Count) {
    throw "Expected $($expected.Count) files but selected $($selected.Count)."
}
Write-Output "PASS: $($selected.Count) unique source files, valid links, mod overrides selected, no Core project reference."
