function is_english()
{
    // Attualmente è utilizzato solo dalle funzioni per il recupero dei dialoghi, quindi preferisco modificare qui perché è centralizzato.
    // Altrimenti dovevamo modificare ogni funzione (che per carità si può fare, ma vedremo più avanti con i prossimi capitoli imo)
    return false; 
    // return !variable_global_exists("lang") || global.lang == "en"; // Codice vecchio
}