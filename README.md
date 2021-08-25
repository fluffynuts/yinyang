# YinYang

### What is this?
A small windows tray utility to switch between light and dark themes

### Why?
- I'd like to work outside in the fresh air and sunshine sometimes
- My display is bright, but not bright enough to compete with outside light
- Windows 10 has dark and light themes
  - Typically, the dark theme suits me best
  - Outside, the light theme is the only way to see things properly
  - Many apps know how to follow the dark/light theme switch
    - Rider / WebStorm / other JetBrains IDE's (configure to follow system theme & select your desired light & dark themes)
    - VSCode
    - Windows Terminal (partial - terminal colors have to be manually tweaked, for now at least)

### Status
Simplest proof-of-concept
- adds a systray icon
    - clicking flips between dark and light
    - there's a menu item for that too, if you prefer
    - notification of the impending switch
- is aware of the current theme at startup
    - is not aware of theme changes outside of the app

### TODO
- react to theme changes outside the app
- icon should animate on change & "flip" such that
    - when light theme, light on top
    - when dark theme, dark on top
- custom handlers for apps which don't switch automatically
    - should update the Windows Terminal selected colorScheme
    - ???
- automatic mode: attempt to determine ambient light &
    automatically switch based on thresholds
    - I'm not sure if this can even happen: my laptop doesn't appear
        to have a light sensor that Windows.Devices picks up
- alt. automatic mode: based on power source
    - switch to light when on battery
        - this is when I'd be outside
    - switch to dark when powered
        - back in the cave!