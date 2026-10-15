Basic quote management system for Streamerbot.
Functionalities: add quotes, return a misc quote, or search for and return a quote by ID.

If using this script, replace this value in quote-management.cs with the path to your own file.
```csharp
const string pathToQuotesDocumentGlobal = @"C:\Users\RainstormWander\source\repos\rw-streamerbot\quotes-custom.json";
```
You'll need to create that file manually before you start using the quote system; the script will not create the file for you.
Required file type extension: .json.
Required file content when you first create it: []
Literally just a set of square brackets like that.

**Command:** !quote
**Trigger:** Core > Commands > Command Triggered
**Sub-actions:**
Add Twitch Broadcaster Information
Twitch > User > Get User Information for Target (user login: %broadcastUserName%)
Execute Code (quote-management.cs)