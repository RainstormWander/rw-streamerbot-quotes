using System;
using System.IO;
using System.Text;

public class CPHInline
{
    // - - - Classes - - - //

    public class Quote {
        
        public string id {get; set;}        
        public string user {get; set;}
        public string moderator {get; set;}
        public string game {get; set;}
        public DateTimeOffset timestamp {get; set;}       
        public string quote {get; set;}
    }

    // - - - Methods - - - //

     /* Purpose: get relevant message data and return them all as strings: 
    - username of person who triggered the command
    - user Id of person who triggered the command
    - game category
    - raw message input */
    public (string, string, string, string) GetMessageInputs()
    {
        string userId = "";
        if (args.TryGetValue("userId", out object userIdObject)) {userId = userIdObject.ToString();} else {/* send message to log */}
        string username = args["userName"].ToString();
        CPH.TryGetArg("game", out string gameCategory);
        string rawInput = args["rawInput"].ToString();
        return (username, userId, gameCategory, rawInput);
    }

    // Purpose: validate whether user is permitted to add quotes
    // use built-in function to fetch user info based on userID
    // track whether user is permitted to post links (ie. a moderator)
    // return result of user validation check
    public bool IsUserPermittedToPostLinks(string userID) {
        var userInfo = CPH.TwitchGetUserInfoById(userID);
        bool userPermitted = userInfo.IsModerator;
        return userPermitted;
    }

    // Purpose: split raw input on white space and return individual words as array
    public string[] SplitMessageText(string rawInput) {
        string[] MessageTextAsArrayOfWords = rawInput.Split();
        return MessageTextAsArrayOfWords; }

    // Purpose: validate whether user is permitted to add quotes and return boolean result
    public bool IsUserPermittedToAddQuotes(string userID) {
        var userInformation = CPH.TwitchGetUserInfoById(userID);
        bool userPermitted = userInformation.IsModerator;
        return userPermitted;
    }

    // Purpose: validate next steps for command execution and return a 'validatedSyntax' variable value which informs next steps
    // we use the value at array indice 0, i.e. first word in message right after "!quote", as that informs our code progression
    public string ReturnValidatedSyntax(string[] MessageTextAsArrayOfWords, bool userPermitted) {
        string validatedSyntax = "";
        if (MessageTextAsArrayOfWords.Length == 0) {validatedSyntax = "noValidatedSyntax";}
        else if (MessageTextAsArrayOfWords[0] == "add" && userPermitted) {validatedSyntax = "addQuote";}
        else if (MessageTextAsArrayOfWords[0] == "add" && !userPermitted) {validatedSyntax = "lockedToMods";}
        else if (MessageTextAsArrayOfWords[0] == "misc") {validatedSyntax = "findMisc";}
        else if (MessageTextAsArrayOfWords[0] == "find") {validatedSyntax = "findId";}
        else {validatedSyntax = "noValidatedSyntax";}
        return validatedSyntax;
    }

    // Purpose: execute next step based on validatedSyntax value; if invalid, indicate in chat; in valid, execute next relevant method
    public void NextStepsForQuoteCommand(string validatedSyntax) {
        if (validatedSyntax == "lockedToMods") {CPH.SendMessage("The add quotes command is locked to moderators.");}
        if (validatedSyntax == "noValidatedSyntax") {CPH.SendMessage("Invalid command. Please use '!quote misc' or '!quote find [quote ID]'.");}      
        if (validatedSyntax == "addQuote") {CPH.SendMessage("Calling method to add quote...");} 
        if (validatedSyntax == "findMisc") {CPH.SendMessage("Calling method to find a misc quote...");} 
        if (validatedSyntax == "findId") {CPH.SendMessage("Calling method to locate a specific quote by Id...");} 
    }

    // Purpose: executes all relevant methods in correct sequence; this is the only thing we'll need to put in Execute() 
    public void ExecuteQuoteCommand() {
        (string username, string userId, string gameCategory, string rawInput) = GetMessageInputs();
        bool userPermitted = IsUserPermittedToPostLinks(userId);
        string[] MessageTextAsArrayOfWords = SplitMessageText(rawInput);
        string validatedSyntax = ReturnValidatedSyntax(MessageTextAsArrayOfWords, userPermitted);
        NextStepsForQuoteCommand(validatedSyntax);
    }

    // Purpose: retrieve the text content of the quotes document and return it as a string
    public string RetrieveFileTextContent() {
        string pathToQuotesDocument = @"C:\Users\RainstormWander\Stream\streamer-bot-1.0.1\data\quotes_custom.json";
        string quotesDocumentTextContent = File.ReadAllText(pathToQuotesDocument);
        return quotesDocumentTextContent;
    }

	public bool Execute()
	{
        ExecuteQuoteCommand();
		return true;
	}
}