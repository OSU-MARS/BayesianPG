library(dplyr)
library(magrittr)
library(r3PG)
library(readxl)
library(tidyr)
library(writexl)

run_3PG_on_site = function(siteName, sites, species, climates, thinning, parameters, sizeDist, settings)
{
  models = tibble(model = c("mix", "pjs27"), number = c(2, 1))
  heightModels = tibble(model = c("exponent", "power"), number = c(2, 1))
  
  site = sites %>% filter(site == siteName)
  climateName = site$climate
  site %<>% select(-site, -climate)
  
  speciesOnSite = species %>% filter(site == siteName) %>%
    select(-site)
  
  climate = climates %>% filter(climate_id == climateName)
  if (all(is.na(climate$tmp_ave)))
  {
    climate %<>% select(-tmp_ave)
  }
  if (all(is.na(climate$d13catm)))
  {
    climate %<>% select(-d13catm)
  }
  
  siteThinning = thinning %>% filter(site == siteName) %>%
    select(-site)
  if (nrow(siteThinning) == 0)
  {
    siteThinning = NULL
  }
  
  speciesParameters = parameters %>% select(parameter, speciesOnSite$species)
  
  speciesSizeDist = sizeDist %>% select(parameter, speciesOnSite$species)
  
  siteSettings = settings %>% filter(site == siteName) %>%
    select(-site, -management) %>%
    rename(correct_bias = correct_sizeDist) %>%
    mutate(light_model = (models %>% filter(model == light_model))$number,
           transp_model = (models %>% filter(model == transp_model))$number,
           phys_model = (models %>% filter(model == phys_model))$number,
           height_model = (heightModels %>% filter(model == height_model))$number,
           correct_bias = if_else(correct_bias == "true", 1, 0),
           calculate_d13c = if_else(calculate_d13c == "true", 1, 0)) %>%
    as.list()
  
  out3PG = run_3PG(site = site, 
                   species = speciesOnSite, 
                   climate = climate, 
                   thinning = siteThinning,
                   parameters = speciesParameters, 
                   size_dist = speciesSizeDist,
                   settings = siteSettings,
                   check_input = TRUE, df_out = TRUE)
  # unique(out3PG$variable)
  out3PG %<>% select(-group) %>% 
    pivot_wider(names_from = variable, values_from = value) %>%
    select(-starts_with("var_")) # suppress unused columns starting with var_ in 4D array
  return(out3PG)
}

sites = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "site")
species = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "species")
climates = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "climate")
thinning = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "thinning")
parameters = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "parameters")
sizeDist = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "sizeDist")
settings = read_xlsx(file.path(getwd(), "UnitTests/r3PG.xlsx"), "settings")

broadleafMix = run_3PG_on_site("broadleaf_mix", sites, species, climates, thinning, parameters, sizeDist, settings)
broadleafPjs = run_3PG_on_site("broadleaf_pjs", sites, species, climates, thinning, parameters, sizeDist, settings)
evergreenMix = run_3PG_on_site("evergreen_mix", sites, species, climates, thinning, parameters, sizeDist, settings)
evergreenPjs = run_3PG_on_site("evergreen_pjs", sites, species, climates, thinning, parameters, sizeDist, settings)
mixturesEurope = run_3PG_on_site("mixtures_eu", sites, species, climates, thinning, parameters, sizeDist, settings)
mixturesOther = run_3PG_on_site("mixtures_other", sites, species, climates, thinning, parameters, sizeDist, settings)

write_xlsx(list("broadleaf_mix" = broadleafMix, "broadleaf_pjs" = broadleafPjs,
                "evergreen_mix" = evergreenMix, "evergreen_pjs" = evergreenPjs,
                "mixtures_eu" = mixturesEurope, "mixtures_other" = mixturesOther), 
           file.path(getwd(), "TestResults/reference.xlsx"))

# figure for # https://github.com/trotsiuk/r3PG/issues/70
ggplot(mixturesOther) +
  geom_path(aes(x = date, y = stems_n, color = species, group = species))
