# houseboatfix
Fixes Sims 3's Island Paradise Houseboats freezing the game/simulation when Sims and Pets deal with entering and exiting

# DESCRIPTION
Fixes IP Houseboats freezing the game/simulation when Sims and Pets deal with entering and exiting.

Something with LAND/WATER pathing is freezing the game/simulation when dealing with Houseboats.

Side effect of this fix is pathing to Houseboats seems have been fixed as well.

Who has thought that two routing options by EA/The Sims Team be the problem? 

I believe this wasn't their intent as in LAND to WATER / WATER to LAND / Whatever is causing pathing be so bugged.

# INSTALLATION
Requires MonoPatcher via Lazy Duchess and Mod Framework (Most likely you already have this) via Mod The Sims.

Place it in ether "Overrides" or "Packages" via Mod Framework.

# CREDITS
foulplay (Foul Play) - Finding the workaround/fix.

phantomsimmer (phantom99) - Very huge helping with a method of debugging the freeze via teleportation of a sim in game.
I would have spent very long time finding a method of debugging the freeze without their huge help.

thesammy58 (Sam) - Proving my silly thinking wrong on which I thought another Method of code were the issue.

unjust_harry ("Just Harry") - Finding the problematic Methods via grep.

fleshtexture (shapes) - For his S3MM inspector, I would not have found a static object that allowed unjust_harry ("Just Harry") to find the broken code.

lazyduchess (Lazy Duchess) - For their incredible MonoPatcher which allowed me to replace the broken Methods.

anime_boom (Anime_Boom) - Very huge help with extensive testing with other mods to make sure it works with other core and script mods.
Also testing mod framework folders to see if it works in both Overrides and Packages. I wouldn't have known about this at all. 
They also tested with Pets and Mermaids.

destrospean (Destrospean) - Helping with code I can use for future projects.

EA/The Sims Team - For their spaghetti code.
