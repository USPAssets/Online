if (instance_exists(obj_mainchara))
    checkX = obj_mainchara.x + 20;

timer += 2;
c_rainbow = make_color_hsv(timer % 255, 255, 255);
curColor = merge_color(merge_color(c_white, c_rainbow, 0.5), c_black, 0.2);
draw_set_color(curColor);

if (createAndStay == 0)
    draw_rectangle(594, 100, 1450, 220, 0);

if (createAndStay == 1)
{
    if (checkX >= 594)
        newcount = 1;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 676)
        newcount = 2;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 716)
        newcount = 3;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 771)
        newcount = 4;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 797)
        newcount = 5;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 841)
        newcount = 6;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 881)
        newcount = 7;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 931)
        newcount = 8;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 986)
        newcount = 9;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1041)
        newcount = 10;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1090)
        newcount = 11;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1116)
        newcount = 12;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1196)
        newcount = 13;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1246)
        newcount = 14;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1266)
        newcount = 15;
    
    if (count < newcount)
        count = newcount;
    
    if (checkX >= 1326)
        newcount = 16;
    
    if (checkX >= 1379)
        newcount = 17;
    
    if (count < newcount)
        count = newcount;
    
    if (count >= 1)
        draw_rectangle(594, 100, 675, 220, 0);
    
    if (count >= 2)
        draw_rectangle(676, 100, 715, 220, 0);
    
    if (count >= 3)
        draw_rectangle(716, 100, 770, 220, 0);
    
    if (count >= 4)
        draw_rectangle(771, 100, 796, 220, 0);
    
    if (count >= 5)
        draw_rectangle(797, 100, 840, 220, 0);
    
    if (count >= 6)
        draw_rectangle(841, 100, 880, 220, 0);
    
    if (count >= 7)
        draw_rectangle(881, 100, 930, 220, 0);
    
    if (count >= 8)
        draw_rectangle(931, 100, 985, 220, 0);
    
    if (count >= 9)
        draw_rectangle(986, 100, 1040, 220, 0);
    
    if (count >= 10)
        draw_rectangle(1041, 100, 1070, 220, 0);
    
    if (count >= 11)
        draw_rectangle(1090, 100, 1115, 220, 0);
    
    if (count >= 12)
        draw_rectangle(1116, 100, 1170, 220, 0);
    
    if (count >= 13)
        draw_rectangle(1196, 100, 1245, 220, 0);
    
    if (count >= 14)
        draw_rectangle(1246, 100, 1265, 220, 0);
    
    if (count >= 15)
        draw_rectangle(1266, 100, 1325, 220, 0);
    
    if (count >= 16)
        draw_rectangle(1326, 100, 1378, 220, 0);
    
    if (count >= 17)
        draw_rectangle(1379, 100, 1450, 220, 0);
    
    if (count == 17)
    {
        if (global.plot < 67)
            global.plot = 67;
    }
}

draw_set_color(c_white);
