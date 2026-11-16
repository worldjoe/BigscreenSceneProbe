# Manual GitHub release

When the user asks to release this mod, use this checklist; never publish
automatically. Project: `BigscreenSceneProbe.csproj`; version source:
`SceneProbe.cs` `MelonInfo`; asset:
`bin\Release\net6.0\BigscreenSceneProbe.dll`.

1. Check the GitHub account, repository and remote, branch, working tree,
   latest release and local and remote tags. If the repository is not set up,
   the working tree is dirty, or the branch is not synchronized with its remote,
   stop and ask how to proceed. Do not silently commit or push changes.
2. Read the version from `MelonInfo`. If the project explicitly sets `Version`,
   confirm it matches. Use `v<version>` as the release tag. If that tag or
   release already exists, stop and ask; never overwrite it or retag a commit.
   Do not bump versions just to release without user approval.
3. Build the clean, pushed commit with the `MelonInfo` version substituted for
   `<version>`:
   `dotnet build .\BigscreenSceneProbe.csproj -c Release --no-incremental -p:Version=<version>`.
   Use the documented `GameDir` override if needed. Verify the DLL's assembly
   and file versions match the release version, with a fourth `.0` component.
   Stop on build or verification failure.
4. Review the changes since the previous release and the README. Prepare
   accurate release notes covering changes, tested game and MelonLoader
   versions, dependencies, installation, and relevant limitations. Do not claim
   in-game testing unless it occurred.
5. Use `gh release create` with the `v<version>` tag, the full pushed commit
   SHA as `--target`, the release notes, and the mod DLL as an asset. Do not
   include game assemblies, secrets, or local build intermediates.
6. Verify the published release, tag, target commit, and asset. Download the
   asset to temporary storage and compare its SHA-256 hash with the built DLL.
   Report the release URL and any testing limitations.
