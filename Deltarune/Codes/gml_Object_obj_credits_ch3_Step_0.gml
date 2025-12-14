if (scr_debug())
{
}

if (timer == 0)
{
    snd_free_all();
    song0 = snd_init("ch2_credits.ogg");
    song1 = mus_play(song0);
}

if (!paused)
    timer++;

if (timer == 100)
    credit_index++;

if (timer == 201)
    credit_index++;

if (timer == 302)
{
    creditalpha = 1;
    credit_index++;
}

if (timer == 403)
    credit_index++;

if (timer == 503)
{
    textalpha = 1;
    credit_index++;
}

if (timer == 604)
{
    creditalpha = 1;
    credit_index++;
}

if (timer == 705)
    credit_index++;

if (timer == 805)
{
    creditalpha = 1;
    credit_index++;
}

if (timer == 906)
    credit_index++;

if (timer == 1007)
    credit_index++;

if (timer == 1108)
    credit_index++;

if (timer == 1208)
    credit_index++;

if (timer == 1309)
    credit_index++;

if (timer == 1410)
    credit_index++;

if (timer == 1511)
    credit_index++;

if (timer == 1611)
{
    textalpha -= 0.01;
    credit_index++;
}

if (timer == 1711)
    credit_index++;

if (timer == 1811)
    credit_index++;

if (timer == 1911)
    credit_index++;

if (timer == 2011)
    credit_index++;

if (timer == 2111)
    credit_index++;

if (timer == 2211)
    credit_index++;

if (timer == 2311)
    credit_index++;

if (timer == 2611)
{
    credit_index++;
    creditalpha = 0;
}

if (timer > (1641 + uspcredits) && timer < (1711 + uspcredits))
{
    if (creditalpha != 1)
        creditalpha += 0.02;
}

if (timer >= (1801 + uspcredits))
{
    creditalpha -= 0.05;
    
    if (creditalpha < -0.1)
        room_goto(room_chapter_continue);
}

if (timer > (1744 + uspcredits) && timer < (1910 + uspcredits))
    creditalpha += 0.02;

if (timer > (1910 + uspcredits))
    creditalpha -= 0.02;

if (keyboard_check_pressed(ord("F")))
    room_speed = 200;

if (keyboard_check_pressed(ord("W")))
    room_speed = 30;
