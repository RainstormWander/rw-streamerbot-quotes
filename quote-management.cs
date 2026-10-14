using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

public class CPHInline
{
    // - - - Classes - - - //

    public class Quote {
        
        // Properties: captured at time of chat message //
        // not all of these properties will be stored in the quotes document
        public string CommandExecutorUsername {get; set;}
        public string CommandExecutorUserId {get; set;}
        public string MessageRawInput {get; set;}        
        public DateTimeOffset MessageTimestamp {get; set;}
        public string GameCategory {get; set;}

        // Properties: derived from captured properties //
        public string QuoteeUsername {get; set;}
        public string ModeratorUsername {get; set;}
        public string QuoteText {get; set;}

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
        else {CPH.SendMessage("Unable to retrieve userId from args.");}
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

    // Purpose: return the username of the moderator who executed the command
    // only to be run if we successfully reach the 'add quote' step, as at that point we have confirmed that command executor was a moderator
    public string ModeratorUsername(string commandExecutorUsername){
        string moderatorUsername = commandExecutorUsername;
        return moderatorUsername;}

    // Purpose: return the username of the person who said the quote
    // second indice in array will be quotee; command syntax is '!quote add [person who said the quote]'
    public string QuoteeUsername(string[] MessageTextAsArrayOfWords) {
        string quoteeUsername = MessageTextAsArrayOfWords[1];
        return quoteeUsername;}

    // Purpose: return the text of the quote (i.e. everything after the quotee's username)
    public string QuoteText(string[] MessageTextAsArrayOfWords) {
        // take the array of words we've made from the chat message and join all the words (on whitespace) into a string, except for the first two words
        // the first two words are the command and the quotee's username, so we want to skip those and just return everything else for the quote text
        string quoteText = string.Join(" ", MessageTextAsArrayOfWords, 2, MessageTextAsArrayOfWords.Length - 2);
        return quoteText;}

    // Purpose: validate whether user is permitted to add quotes and return result of validation check
    // use built-in method to fetch user info based on userID and check whether user is a moderator
    public bool IsUserPermittedToAddQuotes(string commandExecutorUserId) {
        var userInfoById = CPH.TwitchGetUserInfoById(commandExecutorUserId);
        bool userPermitted = userInfoById.IsModerator;
        return userPermitted;}

    // Purpose: validate next steps for command execution and return a 'validatedSyntax' variable value which informs those next steps
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
        if (validatedSyntax == "lockedToMods") {CPH.SendMessage("The '!quote add' command is locked to moderators.");}
        if (validatedSyntax == "noValidatedSyntax") {CPH.SendMessage("Invalid command. Please use '!quote misc' or '!quote find [quote ID]'.");}      
        
        if ((validatedSyntax == "addQuote") || (validatedSyntax == "findMisc") || (validatedSyntax == "findId")) {
                        
            string quotesDocumentTextContent = RetrieveFileTextContent();
            List<Quote> quotesDocumentAsListOfQuoteObjects = DeserializeQuotesDocument(quotesDocumentTextContent); 
            
            if (validatedSyntax == "addQuote") {
                CPH.SendMessage("Calling method to add quote...");
                AddQuoteToDocument(quotesDocumentAsListOfQuoteObjects, MessageTextAsArrayOfWords);}
            if (validatedSyntax == "findMisc") {
                CPH.SendMessage("Calling method to find a misc quote...");
                FindMiscQuote(quotesDocumentTextContent);} 
            if (validatedSyntax == "findId") {
                CPH.SendMessage("Calling method to locate a specific quote by Id...");
                FindQuoteById(quoteId, quotesDocumentTextContent);}}}

    // - - - Methods: Interact with Quotes Document - - - //

    // Purpose: retrieve the text content of the quotes document and return it as a string
        public string RetrieveFileTextContent() {
        string pathToQuotesDocument = @"C:\Users\RainstormWander\source\repos\rw-streamerbot\quotes-custom.json";
        string quotesDocumentTextContent = File.ReadAllText(pathToQuotesDocument);
        return quotesDocumentTextContent;}

    // Purpose: deserialize that string into a list of quote objects and return it
    public List<Quote> DeserializeQuotesDocument(string quotesDocumentTextContent) {
        List<Quote> quotesDocumentAsListOfQuoteObjects = JsonSerializer.Deserialize<List<Quote>>(quotesDocumentTextContent);
        return quotesDocumentAsListOfQuoteObjects;}

    // Purpose: find the highest existing QuoteId in the list, then return the next one as a string
    public int QuoteId(List<Quote> quotesDocumentAsListOfQuoteObjects) {
        int mostRecentQuoteId = 0;
        foreach (Quote quote in quotesDocumentAsListOfQuoteObjects) {if (quote.QuoteId > mostRecentQuoteId) {mostRecentQuoteId = quote.QuoteId;}}
        return mostRecentQuoteId + 1;}

    public void AddQuoteToDocument(List<Quote> quotesDocumentAsListOfQuoteObjects, string[] MessageTextAsArrayOfWords) {
        
        // Keys: QuoteId, MessageTimestamp, ModeratorUsername, QuoteeUsername, GameCategory, QuoteText 
        QuoteId = QuoteId(quotesDocumentAsListOfQuoteObjects);
        DateTimeOffset MessageTimestamp = MessageTimestamp();        
        string ModeratorUsername = ModeratorUsername(CommandExecutorUsername());
        string quoteeUsername = QuoteeUsername(MessageTextAsArrayOfWords);
        string GameCategory = GameCategory();
        string quoteContent = QuoteText(MessageTextAsArrayOfWords);}

    public void FindQuoteById(string quoteId, string quotesDocumentTextContent) {
        // code to find quote by ID
        // search document for specific id 
        // return just the formatted 'quote' value from the document and send to chat message
    }

    public void FindMiscQuote(string quotesDocumentTextContent) {
        // code to find a random misc quote
        // find most recent quote id in quotesDocumentTextContent
        // generate a random number between 1 and most recent quote id (inclusive on both ends)
        // return just the formatted 'quote' value from the document and send to chat message
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