rem https://github.com/StefH/GitHubReleaseNotes

SET version=1.1.4

GitHubReleaseNotes --output ReleaseNotes.md --skip-empty-releases  --exclude-labels question invalid doc wontfix --version %version% --token %GH_TOKEN%