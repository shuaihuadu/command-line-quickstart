//var command = CommandBuilder.BuildRootCommand();
//var command = CommandBuilder.BuildReadCommand();

var command = CommandBuilder.BuildQuotesCommand();

await command.InvokeAsync(args);