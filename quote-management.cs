using System;
using System.IO;
using System.Text;

public class CPHInline
{
    // - - - Classes - - - //

    public class Quote {
        
        // Properties: captured at time of chat message //
        public string CommandExecutorUsername {get; set;}
        public string CommandExecutorUserId {get; set;}
        public string MessageRawInput {get; set;}
        public string GameCategory {get; set;}
        public DateTimeOffset MessageTimestamp {get; set;}       
        // Properties: derived from captured properties //
        public string QuoteeUsername {get; set;}
        public string QuoteContent {get; set;}
        public string ModeratorUsername {get; set;}
        // Properties: remaining quote document fields //
        public string QuoteId {get; set;}        
    }

    // - - - Methods: Retrieve Message Inputs - - - //

    public string MessageRawInput() {
        string messageRawInput = args["rawInput"].ToString();
        return messageRawInput;}

    public string CommandExecutorUserId() {
        string commandExecutorUserId = "";
        if (args.TryGetValue("userId", out object CommandExecutorUserIdObject)) {commandExecutorUserId = CommandExecutorUserIdObject.ToString();}
        else {/* send message to log */}
        return commandExecutorUserId;}

    public string CommandExecutorUsername() {
        string commandExecutorUsername = args["userName"].ToString();
        return commandExecutorUsername;}

    public string GameCategory() {
        CPH.TryGetArg("game", out string gameCategory);
        return gameCategory;}

    public DateTimeOffset MessageTimestamp() {
        DateTimeOffset messageTimestamp = DateTimeOffset.Now;
        return messageTimestamp;}

    // - - - Methods: Parse Message Inputs - - - //
    
    // Purpose: split raw input on white space and return individual words as array
    public string[] SplitMessageText(string messageRawInput) {
        string[] MessageTextAsArrayOfWords = messageRawInput.Split();
        return MessageTextAsArrayOfWords;}

    // only to be run if we successfully reach the 'add quote' step, as at that point we have confirmed that command executor was a moderator
    public string ModeratorUsername(string commandExecutorUsername)
    {
        string moderatorUsername = commandExecutorUsername;
        return moderatorUsername;
    }

    // - - - Methods: Validate User Permissions and Next Steps- - - //

    // Purpose: validate whether user is permitted to add quotes and return result of validation check
    // use built-in method to fetch user info based on userID and check whether user is a moderator
    public bool IsUserPermittedToAddQuotes(string commandExecutorUserId) {
        var userInfoById = CPH.TwitchGetUserInfoById(commandExecutorUserId);
        bool userPermitted = userInfoById.IsModerator;
        return userPermitted;}

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
        return validatedSyntax;}

    // Purpose: execute next step based on validatedSyntax value; if invalid, indicate in chat; in valid, execute next relevant method
    public void NextStepsForQuoteCommand(string validatedSyntax) {
        if (validatedSyntax == "lockedToMods") {CPH.SendMessage("The add quotes command is locked to moderators.");}
        if (validatedSyntax == "noValidatedSyntax") {CPH.SendMessage("Invalid command. Please use '!quote misc' or '!quote find [quote ID]'.");}      
        if (validatedSyntax == "addQuote") {CPH.SendMessage("Calling method to add quote...");} 
        if (validatedSyntax == "findMisc") {CPH.SendMessage("Calling method to find a misc quote...");} 
        if (validatedSyntax == "findId") {CPH.SendMessage("Calling method to locate a specific quote by Id...");}}

    // - - - Methods: Interact with Quotes Document - - - //

    // Purpose: retrieve the text content of the quotes document and return it as a string
    public string RetrieveFileTextContent() {
        string pathToQuotesDocument = @"C:\Users\RainstormWander\Stream\streamer-bot-1.0.1\data\quotes_custom.json";
        string quotesDocumentTextContent = File.ReadAllText(pathToQuotesDocument);
        return quotesDocumentTextContent;
    }

    public void AddQuoteToDocument(Quote newQuote, string quotesDocumentTextContent) {
        // code to add new quote to document
    }

    public void FindQuoteById(string quoteId, string quotesDocumentTextContent) {
        // code to find quote by ID
    }

    public void FindMiscQuote(string quotesDocumentTextContent) {
        // code to find a random misc quote
    }

    // - - - Method: Execute Quote Command - - - //

    // Purpose: executes all relevant methods in correct sequence; this is the only thing we'll need to put in Execute() 
    public void ExecuteQuoteCommand() {
        string username = CommandExecutorUsername();
        string userId = CommandExecutorUserId();
        string gameCategory = GameCategory();
        string rawInput = MessageRawInput();
        bool userPermitted = IsUserPermittedToAddQuotes(userId);
        string[] MessageTextAsArrayOfWords = SplitMessageText(rawInput);
        string validatedSyntax = ReturnValidatedSyntax(MessageTextAsArrayOfWords, userPermitted);
        NextStepsForQuoteCommand(validatedSyntax);
    }

    // - - - Execute Method - - - //

	public bool Execute()
	{
        ExecuteQuoteCommand();
		return true;
	}
}