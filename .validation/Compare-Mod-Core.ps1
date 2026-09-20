param([string]$CorePath = '', [string]$OutputPath = '')
$validation = Split-Path -Parent $PSCommandPath
$mod = (Resolve-Path (Join-Path $validation '..')).Path
if ([string]::IsNullOrWhiteSpace($CorePath)) { $CorePath = Join-Path $mod '..\Core.cpk' }
if ([string]::IsNullOrWhiteSpace($OutputPath)) { $OutputPath = Join-Path $validation 'Mod-Core-Diffs' }
$core = (Resolve-Path $CorePath).Path
if (Test-Path $OutputPath) { Remove-Item -LiteralPath $OutputPath -Recurse -Force }
New-Item -ItemType Directory -Force $OutputPath | Out-Null
$ignore = '(^|[\\/])(\.git|\.vs|bin|obj|\.validation|\.temp)([\\/]|$)'
function Get-Index([string]$root) {
  $result=@{}
  Get-ChildItem $root -Recurse -File | Where-Object {$_.FullName -notmatch $ignore} | ForEach-Object {
    $result[$_.FullName.Substring($root.Length + 1)] = $_.FullName
  }
  return $result
}
$modFiles=Get-Index $mod; $coreFiles=Get-Index $core; $changed=@()
foreach($rel in ($modFiles.Keys | Where-Object {$coreFiles.ContainsKey($_)} | Sort-Object)) {
  if ([IO.Path]::GetFileName($rel) -ieq 'Header.xml') { continue }
  if ((Get-FileHash $modFiles[$rel]).Hash -eq (Get-FileHash $coreFiles[$rel]).Hash) { continue }
  $changed += $rel
  $name=(($rel -replace '[\\/:*?""<>|]','__') + '.diff')
  $target=Join-Path $OutputPath $name
  $left=$coreFiles[$rel]; $right=$modFiles[$rel]
  if ($rel -match '\.(cs|xaml|xml)$') {
    $temp=Join-Path $env:TEMP ('HardcoreDesertDiff-' + [guid]::NewGuid())
    New-Item -ItemType Directory -Force $temp | Out-Null
    $leftTemp=Join-Path $temp 'core.cs'; $rightTemp=Join-Path $temp 'mod.cs'
    $bom = [char]0xFEFF
    $leftLines=Get-Content $left | ForEach-Object { $_.TrimStart($bom) }
    $rightLines=Get-Content $right | ForEach-Object { $_.TrimStart($bom) }
    if ($rel -match '\.cs$') {
      $leftLines=$leftLines | Where-Object {$_ -notmatch '^\s*using\s+.*;'}
      $rightLines=$rightLines | Where-Object {$_ -notmatch '^\s*using\s+.*;'}
    }
    [IO.File]::WriteAllLines($leftTemp, [string[]]$leftLines, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllLines($rightTemp, [string[]]$rightLines, [Text.UTF8Encoding]::new($false))
    $left=$leftTemp; $right=$rightTemp
  }
  git diff --no-index --ignore-all-space --ignore-blank-lines -- $left $right *> $target
  if ($temp) { Remove-Item $temp -Recurse -Force; Remove-Variable temp }
}
$htmlPath=Join-Path (Split-Path $OutputPath) 'Mod-Core-Review.html'
$html=[Text.StringBuilder]::new()
[void]$html.AppendLine('<!doctype html><meta charset="utf-8"><title>Core vs HardcoreDesert</title><style>body{font-family:Segoe UI,Arial;margin:24px;background:#f5f5f5}section{background:#fff;margin:16px 0;padding:12px;border:1px solid #ccc}h2{font-size:15px}pre{font:13px Consolas,monospace;white-space:pre-wrap}.add{background:#dff6dd}.del{background:#ffdede}.hdr{color:#555;font-weight:bold}</style><h1>Core vs HardcoreDesert</h1><p>SOURCE_VALIDATION_RESULT</p><p>Vert&nbsp;: ajout&eacute; au mod. Rouge&nbsp;: supprim&eacute; du Core. Espaces, tabulations et lignes vides ignor&eacute;s.</p>')
foreach($file in (Get-ChildItem $OutputPath -Filter '*.diff' | Sort-Object Name)) {
  $displayName=($file.BaseName -replace '__','\')
  [void]$html.AppendLine('<section><h2>'+[Net.WebUtility]::HtmlEncode($displayName)+'</h2><pre>')
  foreach($line in (Get-Content $file.FullName)) {
    if ($line.StartsWith('diff --git ') -or $line.StartsWith('--- ') -or $line.StartsWith('+++ ') -or $line.StartsWith('index ')) { continue }
    if ($line -match '^Binary files .* differ$') { $line='Binary files differ' }
    $line=$line.Replace($core,'Core').Replace($mod,'HardcoreDesert')
    $line=$line.Replace($core.Replace('\','\\'),'Core').Replace($mod.Replace('\','\\'),'HardcoreDesert')
    $line=$line -replace '(?i)[A-Z]:[\\/]+.*?HardcoreDesertDiff-[^\\/]+[\\/]','Temp/'
    $line=$line -replace '(?i)[A-Z]:\\\\.*?HardcoreDesertDiff-[^\\\\]+\\\\','Temp/'
    $line=$line -replace '(?i)[A-Z]:[\\/][^""<>\r\n]*','Path'
    $class=''; if($line.StartsWith('+') -and !$line.StartsWith('+++')){$class='add'} elseif($line.StartsWith('-') -and !$line.StartsWith('---')){$class='del'} elseif($line.StartsWith('@@') -or $line.StartsWith('diff ')){$class='hdr'}
    [void]$html.AppendLine('<span class="'+$class+'">'+[Net.WebUtility]::HtmlEncode($line)+'</span>')
  }
  [void]$html.AppendLine('</pre></section>')
}
[IO.File]::WriteAllText($htmlPath, $html.ToString(), [Text.UTF8Encoding]::new($true))
Remove-Item -LiteralPath $OutputPath -Recurse -Force
Write-Host "Termine : $($changed.Count) fichiers compares"
Write-Host "Diffs : $OutputPath"
Write-Host "HTML  : $htmlPath"
Write-Host "`nVerification de la selection des sources..."
$projectPath = Join-Path $validation 'HardcoreDesert.Validation.csproj'
$evaluatedJson = & dotnet msbuild $projectPath '-getItem:Compile,Page,ProjectReference'
if ($LASTEXITCODE -ne 0) { throw 'Project evaluation failed.' }
$evaluated = ($evaluatedJson -join "`n") | ConvertFrom-Json
$selected = @($evaluated.Items.Compile) + @($evaluated.Items.Page)
if (@($evaluated.Items.ProjectReference).Count) { throw 'Validation must not reference separately compiled Core/mod projects.' }
$badLinks = @($selected | Where-Object { -not $_.Link -or [System.IO.Path]::IsPathRooted($_.Link) -or $_.Link -match '([\\/]$|(^|[\\/])\.\.([\\/]|$))' })
$duplicateLinks = @($selected | Group-Object Link | Where-Object Count -gt 1)
if ($badLinks.Count -or $duplicateLinks.Count) { throw "Invalid links: $($badLinks.Count); duplicate links: $($duplicateLinks.Count)." }
$expected = @{}
$coreDirectory = (Resolve-Path (Join-Path $validation '../../Core.cpk')).Path
$modDirectory = (Resolve-Path (Join-Path $validation '..')).Path
foreach ($sourceDirectory in @($coreDirectory, $modDirectory)) {
  foreach ($sourceFile in Get-ChildItem -LiteralPath $sourceDirectory -Recurse -File) {
    if ($sourceFile.Extension -notin @('.cs','.xaml')) { continue }
    $relativePath = $sourceFile.FullName.Substring($sourceDirectory.Length + 1)
    if ($relativePath -match '(^|\\)(\.[^\\]*|bin|obj|packages|LibsGameApi)(\\|$)') { continue }
    $expected[$relativePath] = $sourceFile.FullName
  }
}
foreach ($sourceItem in $selected) {
  if (-not $expected.ContainsKey($sourceItem.Link) -or $expected[$sourceItem.Link] -ne $sourceItem.FullPath) { throw "Incorrect source selection: $($sourceItem.Link) -> $($sourceItem.FullPath)" }
}
if ($expected.Count -ne $selected.Count) { throw "Expected $($expected.Count) files but selected $($selected.Count)." }
Write-Output "PASS: $($selected.Count) unique source files, valid links, mod overrides selected, no Core project reference."
$validationText = "PASS: $($selected.Count) sources uniques, liens valides, priorite au mod confirmee, aucune reference directe au Core."
$page = Get-Content -LiteralPath $htmlPath -Raw
$page = $page.Replace('SOURCE_VALIDATION_RESULT', $validationText)
[IO.File]::WriteAllText($htmlPath, $page, [Text.UTF8Encoding]::new($true))
Start-Process $htmlPath


