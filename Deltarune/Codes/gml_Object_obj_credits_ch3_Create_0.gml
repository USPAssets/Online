if (scr_is_switch_os())
    instance_create(0, 0, obj_switchAsyncHelper);

con = 0;
timer = 0;
lyric = stringset(" ");
textalpha = 1;
creditalpha = 1;
trackpos = 0;
modifier = 0;
song1 = 0;
menugray = hexcolor(#949494);
y_pos = __view_get(e__VW.YView, 0);
x_pos = __view_get(e__VW.XView, 0) + (__view_get(e__VW.WView, 0) / 2);
line_height = 20;
title_credit[0] = stringsetloc("DELTARUNE", "obj_credits_ch3_slash_Create_0_gml_18_0");
title_credit[1] = stringsetloc("Chapter 3", "obj_credits_ch3_slash_Create_0_gml_19_0");
title_credit[2] = stringset(" ");
title_credit[3] = stringsetloc("by Toby Fox", "obj_credits_ch3_slash_Create_0_gml_21_0");
continued_text[0] = stringsetloc("To be continued", "obj_credits_ch3_slash_Create_0_gml_23_0");
credit_index = -1;
credits = generate_credits();
credits[15] = [new scr_credit(["", "-Produzione-"], ["Renard", "Jack Lemon"])];
credits[16] = [new scr_credit(["-Traduzione e Check-"], ["BlueJunimo", "Chrippa", "CRefice", "Daizo", "GiMoody", "Jack Lemon", "Vanzzzz", "Willow"])];
credits[17] = [new scr_credit(["-Tech e Tools-"], ["CRefice", "Depa", "Renard"]), new scr_credit(["-Grafica-"], ["Jack Lemon"])];
credits[18] = [new scr_credit(["-Playtesting-"], ["A.CraftDEV", "BetweenShades", "KnuckleBuckle", "L'Armadio", "KendeRoy"]), new scr_credit(["-Discord Server Mods-"], ["Depa", "Vanzzzz"])];
credits[19] = [new scr_credit(["-Sito Web-"], ["Armor", "Gabby", "Spizor"]), new scr_credit(["-Illustrazioni Social-"], ["Chrippa"])];
credits[20] = [new scr_credit(["-Tester Installer-"], ["Giob Caridi", "Matteo Nori", "robinchips"]), new scr_credit(["--Ringraziamenti Speciali--", "-Video Doppiaggio (Tenna)-"], ["IoSonoOtakuman"]), new scr_credit(["-Modding Tools-"], ["Team di sviluppo di UndertaleModTool"])];
credits[21] = [new scr_credit([""], ["Questa traduzione è stata fatta da esseri umani."])];
paused = false;
uspcredits = 600;

enum e__VW
{
    XView,
    YView,
    WView,
    HView,
    Angle,
    HBorder,
    VBorder,
    HSpeed,
    VSpeed,
    Object,
    Visible,
    XPort,
    YPort,
    WPort,
    HPort,
    Camera,
    SurfaceID
}