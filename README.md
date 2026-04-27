# HeroEngine — PP8: Files & Razor Quest

>Maybe you need to set HeroEngine.Web as the default startup project.

## Project structure
- `HeroEngine.Core/` — class library (PP7 models + data persistence)
- `HeroEngine.Web/`  — ASP.NET Core Razor Pages web app


## Data files (HeroEngine.Web/Data/)
| File | Format | Purpose |
|------|--------|---------|
| heroes.json | JSON | Hero persistence (CRUD) |
| combat_stats.csv | CSV | Combat statistics history |
| game_config.xml | XML | Game configuration |
| battle.log | TXT | Append-only combat log |

## Pages
| URL | Description |
|-----|-------------|
| / | Dashboard — hero count & summary |
| /Heroes | All heroes list |
| /Heroes/Detail/{name} | Hero detail + abilities |
| /Heroes/Create | Create & persist new hero |
| /Combat | Run simulated combat, view log |
| /Files | Export/import data, edit config |
| /Stats | LINQ analytics & combat history |