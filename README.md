# Basic quote management system for Streamer.bot
## Functionalities

- Add a new quote with `!quote add <quotee username> <quote text content>` 
- Return a random quote with `!quote misc`
- Find a quote by ID with `!quote find <id>`

Adding quotes is locked to moderators; anyone can use the other two commands.

## Known Limitations

Still a work-in-progress; chat debug messages still included; still needs to be actually tested during live streams.

You'll need to create your quotes file manually before you start using the quote system; the script will not create the file for you and will crash if the file doesn't exist. 

## To-Do

- [ ] Add a check to create that quotes document for the user if the file doesn't exist yet.

## Usage

Create your *quotes-custom.json* file (or whatever you'd like to title it).
- Required file type extension: .json.
- Required text content when you first create the file (just a set of square brackets): `[]`

Replace this value in *quote-management.cs* with the path to your own file.
```csharp
const string pathToQuotesDocumentGlobal = @"C:\Users\RainstormWander\source\repos\rw-streamerbot\quotes-custom.json";
```

- **Command:** !quote
- **Trigger:** Core > Commands > Command Triggered
- **Sub-actions:**
  - Add Twitch Broadcaster Information
  - Twitch > User > Get User Information for Target (user login: %broadcastUserName%)
  - Execute Code (quote-management.cs)