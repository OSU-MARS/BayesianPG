broadleafMixActual = read_actual("broadleaf_mix.csv")
broadleafMixExpected = read_expected("broadleaf_mix")
broadleafMixRatios = get_column_ratios(broadleafMixActual %>% filter(trajectory == 0), broadleafMixExpected)
broadleafMixSummary = summarize_ratios(broadleafMixRatios)

broadleafPjsActual = read_actual("broadleaf_pjs.csv")
broadleafPjsExpected = read_expected("broadleaf_pjs")
broadleafPjsRatios = get_column_ratios(broadleafPjsActual %>% filter(trajectory == 0), broadleafPjsExpected)
broadleafPjsSummary = summarize_ratios(broadleafPjsRatios)

evergreenMixActual = read_actual("evergreen_mix.csv")
evergreenMixExpected = read_expected("evergreen_mix")
evergreenMixRatios = get_column_ratios(evergreenMixActual %>% filter(trajectory == 0), evergreenMixExpected)
evergreenMixSummary = summarize_ratios(evergreenMixRatios)

evergreenPjsActual = read_actual("evergreen_pjs.csv")
evergreenPjsExpected = read_expected("evergreen_pjs")
evergreenPjsRatios = get_column_ratios(evergreenPjsActual %>% filter(trajectory == 0), evergreenPjsExpected)
evergreenPjsSummary = summarize_ratios(evergreenPjsRatios)

mixturesEuropeActual = read_actual("mixtures_eu.csv")
mixturesEuropeExpected = read_expected("mixtures_eu")
mixturesEuropeRatios = get_column_ratios(mixturesEuropeActual %>% filter(trajectory == 0) %>% arrange(species, date), mixturesEuropeExpected)
mixturesEuropeSummary = summarize_ratios(mixturesEuropeRatios)

mixturesOtherActual = read_actual("mixtures_other.csv")
mixturesOtherExpected = read_expected("mixtures_other")
mixturesOtherRatios = get_column_ratios(mixturesOtherActual %>% filter(trajectory == 0) %>% arrange(species, date), mixturesOtherExpected)
mixturesOtherSummary = summarize_ratios(mixturesOtherRatios)

plot_departures(broadleafMixRatios, title = "(a) broadleaf_mix", xLabel = FALSE) +
  plot_departures(broadleafPjsRatios, title = "(b) broadleaf_pjs", xLabel = FALSE, yLabels = FALSE) +
  plot_departures(evergreenMixRatios, title = "(c) evergreen_mix") +
  plot_departures(evergreenPjsRatios, title = "(d) evergreen_pjs", yLabels = FALSE)

plot_departures(mixturesEuropeRatios, title = "(a) mixtures_eu") +
  plot_departures(mixturesOtherRatios, title = "(b) mixtures_other", yLabels = FALSE)

# report
round(as.data.frame(broadleafMixSummary %>% select(-species)), 3)
round(as.data.frame(broadleafPjsSummary %>% select(-species)), 3)
round(as.data.frame(evergreenMixSummary %>% select(-species)), 3)
round(as.data.frame(evergreenPjsSummary %>% select(-species)), 3)
round(as.data.frame(mixturesEuropeSummary %>% select(-species)), 3)
round(as.data.frame(mixturesOtherSummary %>% select(-species)), 3)

# water budget
ggplot() +
  geom_path(aes(x = age, y = asw, color = "actual", group = species), mixturesOtherActual) +
  geom_path(aes(x = age, y = asw, color = "expected", group = species), mixturesOtherExpected) +
  labs(x = NULL, y = "available soil water, mm", color = NULL) +
  theme(legend.position = "none") +
ggplot() +
  geom_path(aes(x = age, y = evapo_transp, color = "actual", group = species), mixturesOtherActual) +
  geom_path(aes(x = age, y = evapo_transp, color = "expected", group = species), mixturesOtherExpected) +
  labs(x = NULL, y = "total evapotranspiration, mm", color = NULL) +
  theme(legend.position = "none") +
ggplot() +
  geom_path(aes(x = age, y = evapotra_soil, color = "actual", group = species), mixturesOtherActual) +
  geom_path(aes(x = age, y = evapotra_soil, color = "expected", group = species), mixturesOtherExpected) +
  labs(x = "species age, years", y = "soil evapotranspiration, mm", color = NULL) +
  theme(legend.position = "none") +
ggplot() +
  geom_path(aes(x = age, y = prcp_runoff, color = "actual", group = species), mixturesOtherActual) +
  geom_path(aes(x = age, y = prcp_runoff, color = "expected", group = species), mixturesOtherExpected) +
  labs(x = "species age, years", y = "runoff, mm", color = NULL) +
  theme(legend.justification = c(1, 1), legend.position = c(0.98, 0.98))
  
# density-dependent mortality
ggplot() +
  geom_path(aes(x = age, y = stems_n, color = "actual", group = species), mixturesEuropeActual) +
  geom_path(aes(x = age, y = stems_n, color = "expected", group = species), mixturesEuropeExpected) +
  labs(x = NULL, y = "TPH", color = NULL) +
  theme(legend.position = "none") +
ggplot() +
  geom_path(aes(x = age, y = basal_area_prop, color = "actual", group = species), mixturesEuropeActual) +
  geom_path(aes(x = age, y = basal_area_prop, color = "expected", group = species), mixturesEuropeExpected) +
  labs(x = NULL, y = "basal area proportion", color = NULL) +
  theme(legend.position = "none") +
ggplot() +
  geom_path(aes(x = age, y = biom_tree_max, color = "actual", group = species), mixturesEuropeActual) +
  geom_path(aes(x = age, y = biom_tree_max, color = "expected", group = species), mixturesEuropeExpected) +
  labs(x = "species age, years", y = "maximum tree biomass, kg", color = NULL) +
  theme(legend.position = "none") +
ggplot() +
  geom_path(aes(x = age, y = mort_thinn, color = "actual", group = species), mixturesEuropeActual) +
  geom_path(aes(x = age, y = mort_thinn, color = "expected", group = species), mixturesEuropeExpected) +
  labs(x = "species age, years", y = "thinning mortality, TPH", color = NULL) +
  theme(legend.justification = c(1, 1), legend.position = c(0.98, 0.98))
