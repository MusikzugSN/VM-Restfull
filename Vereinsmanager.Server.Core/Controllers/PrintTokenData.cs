namespace Vereinsmanager.Controllers;

// Was im Cache zu einem Druck-Token gespeichert wird
public record PrintTokenData(int[] MusicSheetIds, int? MusicFolderId);

// Was die Website an POST /print/token schickt
public record CreatePrintTokenRequest(int[]? MusicSheetIds, int? MusicFolderId);