module ArffLanguageUtilities

open System.IO

let parseFile filePath =
    use reader = new StreamReader(File.OpenRead(filePath))
    let lexbuf = Lexing.LexBuffer<_>.FromTextReader (reader)
    try
        ArffLanguageParser.start ArffLanguageLex.tokenize lexbuf
    with e ->
        let pos = lexbuf.EndPos 
        failwithf "Error near line %d, character %d\n" pos.Line pos.Column