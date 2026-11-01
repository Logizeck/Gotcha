namespace Gotcha.Progress
{
    /// <summary>
    /// Ordine dei livelli fino alla 0.75 (276 livelli, per identificativo). Serve solo a convertire i vecchi
    /// salvataggi, che ricordavano "la posizione più alta sbloccata", nell'elenco dei livelli superati (dalla 0.76 i
    /// progressi sono legati all'identificativo del livello, non alla sua posizione). Non va mai modificato.
    /// </summary>
    public static class LegacyOrder
    {
        public static readonly string[] V075 =
        {
            "key_door", "tilt_maze", "do_nothing", "slice_apple", "beep_code", "defuse",
            "shush", "hold_both", "bug_spray", "art_theft", "snowman", "defend_cat",
            "you_won", "cookie_jar", "birthday", "trap_maze", "dark_maze", "hidden_safe",
            "bedtime", "lift_off", "low_battery", "hot_soup", "frozen_key", "campfire",
            "toni_stuck", "alarm_clock", "thirsty_plant", "shell_game", "sky_wire", "sunny_day",
            "cut_rope", "pizza_party", "pizza_chef", "balloon", "cat_eyes", "pancake",
            "sting_run", "secret_code", "apple_catch", "marble_run", "copycat", "zig_zag",
            "pancake_tower", "shell_shock", "code_breaker", "paper_fold", "bodyguard_2", "big_balloon",
            "trap_maze_2", "toni_tug", "big_birthday", "high_plant", "copycat_pro", "wasp_returns",
            "marble_run_2", "laser_pointer", "tiny_mouse", "snooze_alarm", "darker", "apple_rain",
            "wet_wood", "flashlight", "double_sting", "bridge_builder", "short_fuse", "copycat_maestro",
            "trap_maze_3", "magic_rune", "foggy_mirror", "fast_beeps", "pitch_black", "shell_storm",
            "dj_mouse", "bodyguard_3", "toni_hide", "wasp_hall", "two_doors", "master_code",
            "thirsty_tree", "plumber", "fake_coin", "spot_it", "where_mouse", "tightrope",
            "shadow_play", "wasp_king", "staring_contest", "card_house", "paper_plane", "safe_cracker",
            "carousel", "slide_puzzle", "domino", "mirror_path", "magnet_fishing", "cuckoo_clock",
            "fireflies", "cheating_cat", "bubble_count", "toni_cake", "sneaky_mouse", "blind_stopwatch",
            "whack_a_mole", "bulb_grid", "color_tubes", "four_liters", "untangle", "dot_to_dot",
            "sky_crane", "gone_fishing", "count_sheep", "lock_screen", "tall_guest", "box_pusher",
            "hungry_snake", "pinball", "air_hockey", "slam_dunk", "knock_knock", "screen_time",
            "broken_vase", "word_salad", "why_chicken", "paint_mixer", "toni_dream", "bubble_wrap",
            "out_of_tune", "claw_machine", "morning_routine", "bank_heist", "submarine", "haunted_house",
            "kitchen_escape", "space_station", "treasure_map", "detective", "bomb_squad", "potion_lab",
            "robot_factory", "train_switches", "garden_seasons", "museum_night", "ice_cream_shop", "pirate_cannon",
            "elevator_logic", "circuit_board", "snow_globe", "knight_quest", "the_gauntlet", "toni_fetch",
            "dont_touch_me", "balloon_surgery", "the_lazy_elevator", "phone_sandwich", "left_behind", "sleeping_guard",
            "the_broken_compass", "fake_update", "glass_floor", "tiny_thief", "the_silent_alarm", "black_hole",
            "gravity_resigned", "the_cheap_magician", "dont_wake_the_bomb", "the_upside_down_king", "one_too_many", "reverse_parking",
            "the_stubborn_door", "zero_gravity_dinner", "the_forgotten_password", "the_shrinking_room", "paperwork_nightmare", "battery_vampire",
            "toni_xray", "the_fourth_wall", "hot_potato", "shake_to_undo", "the_lonely_cloud", "ghost_selfie",
            "magnet_mayhem", "the_fake_crash", "rainbow_thief", "pixel_rebellion", "cold_coffee", "the_missing_second",
            "mirror_twins", "the_hungry_printer", "finger_maze", "false_finish", "heartbeat", "the_wrong_shadow",
            "the_impossible_scale", "rewind_reality", "the_sticky_finger", "the_guilty_button", "the_invisible_stairs", "lie_detector",
            "too_bright", "toni_beauty_filter", "unfair_race", "the_bottomless_cup", "the_dirty_secret", "the_fake_virus",
            "slow_motion", "the_overworked_robot", "the_missing_letter", "anti_gravity_taxi", "the_silent_disco", "the_door_that_lies",
            "invisible_button", "the_second_key", "shadow_thief", "the_broken_ruler", "the_backward_song", "dont_blink",
            "the_stupid_genius", "the_escaping_cursor", "upside_down_code", "the_human_charger", "adpocalypse", "the_glass_cage",
            "the_invisible_ink", "the_last_drop", "toni_personal_trainer", "the_angry_keyboard", "the_cowardly_dragon", "hidden_frequency",
            "the_crooked_bridge", "the_final_wrong_answer", "the_impatient_clock", "the_frozen_waterfall", "the_fake_checkpoint", "the_two_finger_pianist",
            "the_moon_thief", "back_to_the_future", "the_three_idiots", "the_shy_star", "the_broken_internet", "the_laughing_lock",
            "the_fake_boss", "the_great_escape", "the_impossible_shot", "the_endless_staircase", "hw_werepug", "hw_pumpkin",
            "hw_ghost", "hw_witch", "hw_zombie", "hw_trick_or_treat", "v64_fingerprint_thief", "v62_wake_kitty",
            "v64_plastic_fish", "v64_neighbours_alarm", "v64_invisible_password", "v64_incompetent_magician", "v64_perfect_liar", "v64_rigged_cards",
            "almost_there", "toni_final_exam", "v67_dark_room", "v67_immortal_candle", "v68_stuck_snack", "v68_camera_shy_bird",
            "v69_dont_let_it_sink", "v69_crooked_picture", "v70_unswattable_fly", "v70_stubborn_ketchup", "v72_cat_burglar", "v72_lost_remote",
            "v73_stuck_drawer", "v73_missing_sock", "v74_piggy_bank", "v74_crying_onion", "v75_toni_armchair", "v75_locked_bike",
        };
    }
}
