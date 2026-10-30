using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

public class CPHInline
{

    // - - - Global Variables - - - //

    const string pathToQuotesDocumentGlobal = @"C:\Users\RainstormWander\source\repos\rw-streamerbot-quotes\quotes-custom.json";

    // - - - Classes - - - //

    public class Quote {
        
        // Properties: captured at time of chat message //
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
        public int QuoteId {get; set;}        
    }

    // - - - Methods: Retrieve Message Inputs - - - //

    public string GetMessageRawInput() {
        string messageRawInput = args["rawInput"].ToString();
        return messageRawInput;}

    public string GetCommandExecutorUserId() {
        string commandExecutorUserId = "";
        if (args.TryGetValue("userId", out object CommandExecutorUserIdObject)) {commandExecutorUserId = CommandExecutorUserIdObject.ToString();}
        else {CPH.SendMessage("Unable to retrieve userId from args.");}
        return commandExecutorUserId;}

    public string GetCommandExecutorUsername() {
        string commandExecutorUsername = args["userName"].ToString();
        return commandExecutorUsername;}

    public string GetGameCategory() {
        CPH.TryGetArg("game", out string gameCategory);
        return gameCategory;}

    public DateTimeOffset GetMessageTimestamp() {
        DateTimeOffset messageTimestamp = DateTimeOffset.Now;
        return messageTimestamp;}

    // - - - Methods: Parse Message Inputs - - - //
    
    // Purpose: split raw input on white space and return individual words as array
    public string[] SplitMessageText(string messageRawInput) {
        string[] MessageTextAsArrayOfWords = messageRawInput.Split();
        return MessageTextAsArrayOfWords;}

    // Purpose: return the username of the moderator who executed the command
    // only to be run if we successfully reach the 'add quote' step, as at that point we have confirmed that command executor was a moderator
    public string GetModeratorUsername(string commandExecutorUsername){
        string moderatorUsername = commandExecutorUsername;
        return moderatorUsername;}

    // Purpose: return the username of the person who said the quote
    // second indice in array will be quotee; command syntax is '!quote add [person who said the quote]'
    // trim whitespace from quoteeUsername, and remove any at symbol(s) at the start of quoteeUsername
    public string GetQuoteeUsername(string[] MessageTextAsArrayOfWords) {
        string quoteeUsername = MessageTextAsArrayOfWords[1];
        return quoteeUsername.Trim().TrimStart('@');}

    // Purpose: return the text of the quote (i.e. everything after the quotee's username)
    public string GetQuoteText(string[] MessageTextAsArrayOfWords) {
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
    public void NextStepsForQuoteCommand(string validatedSyntax, string[] MessageTextAsArrayOfWords) {   
        if (validatedSyntax == "lockedToMods") {CPH.SendMessage("The '!quote add' command is locked to moderators.");}
        if (validatedSyntax == "noValidatedSyntax") {CPH.SendMessage("Invalid command. Please use '!quote misc' or '!quote find [quote ID]'.");}      
        
        if ((validatedSyntax == "addQuote") || (validatedSyntax == "findMisc") || (validatedSyntax == "findId")) {
                        
            string quotesDocumentTextContent = RetrieveFileTextContent();
            List<Quote> quotesDocumentAsListOfQuoteObjects = DeserializeQuotesDocument(quotesDocumentTextContent); 
            
            if (validatedSyntax == "addQuote") {
                // CPH.SendMessage("Calling method to add quote...");
                AddQuoteToDocument(quotesDocumentAsListOfQuoteObjects, MessageTextAsArrayOfWords);}
            if (validatedSyntax == "findMisc") {
                // CPH.SendMessage("Calling method to find a misc quote...");
                FindMiscQuote(quotesDocumentAsListOfQuoteObjects);} 
            if (validatedSyntax == "findId") {
                // CPH.SendMessage("Calling method to locate a specific quote by Id...");
                // the second indice in the array will be the quote id; command syntax is '!quote find [quote id]'
                if (MessageTextAsArrayOfWords.Length > 1 && int.TryParse(MessageTextAsArrayOfWords[1], out int quoteId)) {
                    FindQuoteById(quotesDocumentAsListOfQuoteObjects, quoteId); } 
                    else {CPH.SendMessage("Quote ID is not a valid number.");}}}}

    // - - - Methods: Interact with Quotes Document - - - //

    // Purpose: retrieve the text content of the quotes document and return it as a string
    public string RetrieveFileTextContent() {
        string quotesDocumentTextContent = File.ReadAllText(pathToQuotesDocumentGlobal);
        return quotesDocumentTextContent;}

        // Purpose: deserialize that string into a list of quote objects and return it
        public List<Quote> DeserializeQuotesDocument(string quotesDocumentTextContent) {
        List<Quote> quotesDocumentAsListOfQuoteObjects = JsonConvert.DeserializeObject<List<Quote>>(quotesDocumentTextContent);
        return quotesDocumentAsListOfQuoteObjects;}

    // Purpose: get current highest quote id in the quotes document
    public int GetCurrentHighestQuoteId(List<Quote> quotesDocumentAsListOfQuoteObjects) {
        int mostRecentQuoteId = 0; 
        foreach (Quote quote in quotesDocumentAsListOfQuoteObjects) {if (quote.QuoteId > mostRecentQuoteId) {mostRecentQuoteId = quote.QuoteId;}}
        return mostRecentQuoteId;}

    // Purpose: find the highest existing QuoteId in the list, then increment by 1 and return updated quote id to user with new quote
    public int GenerateNewQuoteId(List<Quote> quotesDocumentAsListOfQuoteObjects) {
        int mostRecentQuoteId = GetCurrentHighestQuoteId(quotesDocumentAsListOfQuoteObjects);
        int newQuoteId = mostRecentQuoteId + 1;
        return newQuoteId;}

    // - - - Methods: Action Quotes Document (Add, Find ID, Find Misc) - - - //

    public void AddQuoteToDocument(List<Quote> quotesDocumentAsListOfQuoteObjects, string[] MessageTextAsArrayOfWords) {
        // Keys: QuoteId, MessageTimestamp, ModeratorUsername, QuoteeUsername, GameCategory, QuoteText 
        int QuoteId = GenerateNewQuoteId(quotesDocumentAsListOfQuoteObjects);
        DateTimeOffset MessageTimestamp = GetMessageTimestamp();        
        string ModeratorUsername = GetModeratorUsername(GetCommandExecutorUsername());
        string quoteeUsername = GetQuoteeUsername(MessageTextAsArrayOfWords);
        string GameCategory = GetGameCategory();
        string quoteContent = GetQuoteText(MessageTextAsArrayOfWords);
        
        var newQuote = new Quote {
            CommandExecutorUsername = GetCommandExecutorUsername(),
            CommandExecutorUserId = GetCommandExecutorUserId(),
            MessageRawInput = GetMessageRawInput(),
            QuoteId = QuoteId,
            MessageTimestamp = MessageTimestamp,
            ModeratorUsername = ModeratorUsername,
            QuoteeUsername = quoteeUsername,
            GameCategory = GameCategory,
            QuoteText = quoteContent};
            
            quotesDocumentAsListOfQuoteObjects.Add(newQuote);
            string updatedJson = JsonConvert.SerializeObject(quotesDocumentAsListOfQuoteObjects, Formatting.Indented);
            File.WriteAllText(pathToQuotesDocumentGlobal, updatedJson);
            CPH.SendMessage($"Quote {newQuote.QuoteId}: {newQuote.QuoteText} - {newQuote.QuoteeUsername} | {newQuote.GameCategory} | {newQuote.MessageTimestamp.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)} | Added by {newQuote.ModeratorUsername}.");}
        
    public void FindQuoteById(List<Quote> quotesDocumentAsListOfQuoteObjects, int quoteId){
        foreach (Quote quote in quotesDocumentAsListOfQuoteObjects) {
            if (quote.QuoteId == quoteId) {
            CPH.SendMessage($"Quote {quote.QuoteId}: {quote.QuoteText} - {quote.QuoteeUsername} | {quote.GameCategory} | {quote.MessageTimestamp.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)} | Added by {quote.ModeratorUsername}.");
            return;}}
        CPH.SendMessage($"Quote {quoteId} not found.");}

    public void FindMiscQuote(List<Quote> quotesDocumentAsListOfQuoteObjects){
        Random random = new Random();
        int randomIndex = random.Next(0, quotesDocumentAsListOfQuoteObjects.Count);
        Quote quote = quotesDocumentAsListOfQuoteObjects[randomIndex];
        CPH.SendMessage($"Quote {quote.QuoteId}: {quote.QuoteText} - {quote.QuoteeUsername} | {quote.GameCategory} | {quote.MessageTimestamp.ToString("MMMM d, yyyy", System.Globalization.CultureInfo.InvariantCulture)} | Added by {quote.ModeratorUsername}.");}

    // - - - Method: Execute Quote Command - - - //

    // Purpose: executes all relevant methods in correct sequence; this is the only thing we'll need to put in Execute() 
    public void ExecuteQuoteCommand() {
        string username = GetCommandExecutorUsername();
        string userId = GetCommandExecutorUserId();
        string gameCategory = GetGameCategory();
        string rawInput = GetMessageRawInput();
        bool userPermitted = IsUserPermittedToAddQuotes(userId);
        string[] MessageTextAsArrayOfWords = SplitMessageText(rawInput);
        string validatedSyntax = ReturnValidatedSyntax(MessageTextAsArrayOfWords, userPermitted);
        NextStepsForQuoteCommand(validatedSyntax, MessageTextAsArrayOfWords);
    }

    // - - - Execute Method - - - //

	public bool Execute()
	{
        ExecuteQuoteCommand();
		return true;
	}
}