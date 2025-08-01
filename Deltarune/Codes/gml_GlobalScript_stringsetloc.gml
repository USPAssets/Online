function stringsetloc(arg0, arg1)
{
    var str = arg0;
    
    // Aggiunto questo if perché toby vuole caricare (inutilmente) prima i nomi dei bordi
    // Tanto su PC non ci sono bordi, quindi ritorno un default ""
    if (!variable_global_exists("lang_map"))
        return "";
    
    if (!is_english())
        str = scr_84_get_lang_string(arg1);
    
    return stringset(str);
}