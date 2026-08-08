/**
 *  Undertale Spaghetti Project
 *  DELTARUNEDemo Check Script
 *
 *  @author USP
 */

async Task<bool> DoCheck()
{
	EnsureDataLoaded();

	if (ScriptPath is null) {
		throw new Exception("Errore nel caricare lo script di check.");
	}

    // For now, we just do a name check, we can always add more checks as needed.
	var displayName = Data.GeneralInfo?.DisplayName?.Content;
    return displayName.Equals("deltarune chapter 1&2", StringComparison.InvariantCultureIgnoreCase);
}

return await DoCheck();