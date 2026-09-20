# Audio settings regression checks

Run `dotnet run --project Tests/AudioSettings/AudioSettings.csproj` from the repository root.

Executes the production bootstrap and settings manager with small Unity substitutes. Covers direct scene startup, duplicate rejection, persistence of the whole audio prefab, mixer initialization after Awake, dynamic slider binding, synchronization, mute, scene transitions, and saved volume. Audible playback and VR interaction still require Play Mode.
