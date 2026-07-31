if (!loaded)
    exit;

if (scr_debug())
{
    if (sunkus_kb_check_pressed(ord("R")))
    {
        snd_free_all();
        room_restart();
    }
    
    if (sunkus_kb_check_pressed(ord("Q")))
        audio_sound_set_track_position(song1, audio_sound_get_track_position(song1) + measure_time);
    
    if (sunkus_kb_check_pressed(vk_space))
    {
        if (!paused)
        {
            paused = true;
            audio_pause_all();
        }
        else
        {
            paused = false;
            audio_resume_all();
        }
    }
}

if (!init)
{
    init = true;
    snd_free_all();
    song0 = snd_init("ch4_credits.ogg");
    song1 = mus_play(song0);
}

timer++;

if (audio_is_playing(song1))
{
    var timer_progress = timer / (audio_sound_length(song1) * 30);
    var sync_amount = timer_progress - (audio_sound_get_track_position(song1) / audio_sound_length(song1));
    
    if (sync_amount >= 0.03)
        audio_sound_set_track_position(song1, audio_sound_length(song1) * timer_progress);
}

if (credits_con == 0)
{
    if (credit_index < (array_length(credits) - 1))
    {
        if (audio_is_playing(song1))
        {
            var track_progress = audio_sound_get_track_position(song1);
            
            if (track_progress < measure_timer)
                exit;
            
            var measure_progress = track_progress / measure_time;
            credit_index = floor(measure_progress) - 1;
        }
    }
    else
    {
        credits_con = 1;
        glowing_active = true;
    }
}

if (credits_con == 1)
{
    var track_progress = audio_sound_get_track_position(song1);
    
    if (track_progress >= 59.75)
    {
        creditalpha = 0;
        credits_con = -1;
    }
}

if (glowing_active)
{
    if (con == 0)
    {
        con = 1;
        auto_text = true;
    }
    
    if (con == 50 && !i_ex(obj_writer))
    {
        con = 51;
        scr_delay_var("con", 52, 30);
    }
    
    if (con == 52 && !i_ex(obj_writer))
    {
        con = 53;
        scr_delay_var("con", 54, 90);
        credit_index++;
        creditalpha = 1;
        scr_lerpvar("year_alpha", -1, 1, 30);
    }
    
    if (con == 54)
    {
        con = -1;
        scr_delay_var("con", 55, 90);
        scr_lerpvar("year_alpha", 1, 0, 30);
    }
    
    if (con == 55 && !i_ex(obj_writer))
    {
        if (audio_is_playing(song1))
        {
            var track_progress = audio_sound_get_track_position(song1);
            var measure_progress = track_progress / measure_time;
            var current_measure = floor(measure_progress);
            
            if (current_measure == 26)
            {
                con = 60;
                creditalpha = 0;
            }
        }
        else
        {
            con = 60;
            creditalpha = 0;
        }
    }
    
    if (con == 60)
    {
        usp_moment = true;
        con = -1;
        credit_index = 0;
    }
    
    if (auto_text)
    {
        if (audio_is_playing(song1))
        {
            var track_progress = audio_sound_get_track_position(song1);
            
            if (track_progress >= auto_text_start)
            {
                if (!i_ex(obj_writer))
                {
                    if (glowing_index < array_length(glowing_text))
                    {
                        dequeue_text();
                    }
                    else
                    {
                        auto_text = false;
                        con = 50;
                    }
                }
            }
            
            if (track_progress >= auto_text_stop)
            {
                with (obj_writer)
                    forcebutton1 = 1;
            }
            
            with (obj_writer)
                skippable = 0;
        }
    }
}

if (usp_moment)
{
    usp_timer++;
    credits = usp_credits;
    glowing_active = false;
    creditalpha = 1;
    
    if (usp_timer == 100)
        credit_index++;
    
    if (usp_timer == 200)
        credit_index++;
    
    if (usp_timer == 300)
        credit_index++;
    
    if (usp_timer == 400)
        credit_index++;
    
    if (usp_timer == 500)
        credit_index++;
    
    if (usp_timer == 600)
        credit_index++;
    
    if (usp_timer == 700)
        credit_index++;
    
    if (usp_timer == 800)
        credit_index++;
    
    if (usp_timer == 900)
        creditalpha = 0;
    
    if (usp_timer == 1000)
        room_goto(room_chapter_continue);
}
