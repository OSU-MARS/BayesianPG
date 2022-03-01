$unitTestDirectory = ([System.IO.Path]::Combine($env:USERPROFILE, "Documents\PhD\BayesianPG\UnitTests"))
$buildDirectory = ([System.IO.Path]::Combine($unitTestDirectory, "bin\x64\Debug\net6.0"))
#$buildDirectory = ([System.IO.Path]::Combine($unitTestDirectory, "bin\x64\Release\net6.0"))
$outputDirectory = ([System.IO.Path]::Combine($unitTestDirectory, "..\TestResults"))

Import-Module -Name ([System.IO.Path]::Combine($buildDirectory, "BayesianPG.dll"));

$stands = Get-StandTrajectories -Xlsx ([System.IO.Path]::Combine($unitTestDirectory, "r3PG validation stands.xlsx"))
Write-StandTrajectory -ReferencePrecision -Trajectory $stands["broadleaf_mix"].Trajectory -File ([System.IO.Path]::Combine($outputDirectory, "broadleaf_mix.csv"))
Write-StandTrajectory -ReferencePrecision -Trajectory $stands["broadleaf_pjs"].Trajectory -File ([System.IO.Path]::Combine($outputDirectory, "broadleaf_pjs.csv"))
Write-StandTrajectory -ReferencePrecision -Trajectory $stands["evergreen_mix"].Trajectory -File ([System.IO.Path]::Combine($outputDirectory, "evergreen_mix.csv"))
Write-StandTrajectory -ReferencePrecision -Trajectory $stands["evergreen_pjs"].Trajectory -File ([System.IO.Path]::Combine($outputDirectory, "evergreen_pjs.csv"))
Write-StandTrajectory -ReferencePrecision -Trajectory $stands["mixtures_eu"].Trajectory -File ([System.IO.Path]::Combine($outputDirectory, "mixtures_eu.csv"))
Write-StandTrajectory -ReferencePrecision -Trajectory $stands["mixtures_other"].Trajectory -File ([System.IO.Path]::Combine($outputDirectory, "mixtures_other.csv"))