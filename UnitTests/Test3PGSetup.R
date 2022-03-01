library(dplyr)
library(ggplot2)
library(patchwork)
library(readr)
library(readxl)
library(tidyr)

theme_set(theme_bw() + theme(axis.line = element_line(size = 0.25),
                             legend.background = element_rect(fill = alpha("white", 0.5)),
                             panel.border = element_blank(),
                             title = element_text(size = 10)))

get_column_ratios = function(actual, expected)
{
  actualColumns = colnames(actual)
  expectedColumns = colnames(actual)
  
  if ("Year & month" %in% colnames(expected))
  {
    # legacy case: expected values read from r3PG_input.xls in https://github.com/trotsiuk/r3PG/tree/master/pkg/tests/r_vba_compare
    # if needed, add support for second species
    indices = 2:(nrow(expected) - 1)
    ratios = tibble(species = "1",
      aero_resist = actual$aero_resist[indices] / expected$`TODO`[indices],
      age = actual$age[indices] / expected$`Age species1`[indices],
      alpha_c = actual$alpha_c[indices] / expected$alphaC1[indices],
      basal_area = actual$basal_area[indices] / expected$`Basal area1`[indices],
      basal_area_prop = actual$basal_area_prop[indices] / expected$`TODO`[indices],
      biom_foliage = actual$biom_foliage[indices] / expected$`Foliage DM1`[indices],
      biom_root = actual$biom_root[indices] / expected$`Root DM1`[indices],
      biom_stem = actual$biom_stem[indices] / expected$`Stem DM1`[indices],
      biom_tree = actual$biom_tree[indices] / expected$`TODO`[indices],
      biom_tree_max = actual$biom_tree_max[indices] / expected$`TODO`[indices],
      canopy_vol_frac = actual$canopy_vol_frac[indices] / expected$`TODO`[indices],
      conduct_canopy = actual$conduct_canopy[indices] / expected$`Canopy conductance1`[indices],
      dbh = actual$dbh[indices] / expected$`Mean DBH1`[indices],
      epsilon_gpp = actual$epsilon_gpp[indices] / expected$Epsilon1[indices],
      epsilon_npp = actual$epsilon_npp[indices] / expected$NPPEpsilon1[indices],
      #fracBB = actual$fracBB[indices] / expected$`Bark & branch fraction1`[indices],
      f_age = actual$f_age[indices] / expected$fAge1[indices],
      f_calpha = actual$f_calpha[indices] / expected$fCalpha1[indices],
      f_cg = actual$f_cg[indices] / expected$fCg1[indices],
      f_frost = actual$f_frost[indices] / expected$fFrost1[indices],
      f_phys = actual$f_phys[indices] / expected$PhysMod1[indices],
      f_nutr = actual$f_nutr[indices] / expected$fNutr1[indices],
      f_sw = actual$f_sw[indices] / expected$fSW1[indices],
      f_tmp = actual$f_tmp[indices] / expected$fT1[indices],
      f_tmp_gc = actual$f_tmp_gc[indices] / expected$fTgc1[indices],
      f_vpd = actual$f_vpd[indices] / expected$fVPD1[indices],
      gammaF = actual$gammaF[indices] / expected$gammaF1[indices],
      #gammaN = actual$gammaN[indices] / expected$gammaN1[indices],
      height = actual$height[indices] / expected$Height1[indices],
      lai = actual$lai[indices] / expected$LAI1[indices],
      prcp_interc = actual$prcp_interc[indices] / expected$TODO[indices],
      sla = actual$sla[indices] / expected$SLA1[indices],
      stems_n = actual$stems_n[indices] / expected$Stems1[indices],
      volume = actual$volume[indices] / expected$`Stand volume1`[indices],
      vpd_sp = actual$vpd_sp[indices] / expected$`VPD_sp`,
      asw = actual$asw[indices] / expected$ASW[indices],
      conduct_soil = actual$conduct_soil / expected$`Soil conductance`[indices],
      evapo_transp = actual$evapo_transp / expected$`TODO`[indices],
      evapotra_soil = actual$evapotra_soil / expected$`Soil evaporation`[indices],
      irrig_supl = actual$irrig_supl[indices] / expected$`Supp. irrig.`[indices],
      prcp_runoff = actual$prcp_runoff[indices] / expected$`Run off`[indices],
      #vol_cum = actual$vol_cum[indices] / expected$`Total cumulative volume1`[indices],
      #wood_density = actual$wood_density[indices] / expected$`Basic density1`[indices],
      #wue = actual$wue[indices] / expected$WUE1[indices],
      #wue_transp = actual$wue_transp[indices] / expected$WUEtransp1[indices],
      )
  }
  else
  {
    ratios = tibble(species = actual$species,
        aero_resist = if_else((actual$aero_resist != 0) & (expected$aero_resist != 0), actual$aero_resist / expected$aero_resist, 1),
        age = if_else((actual$age != 0) & (expected$age != 0), actual$age / expected$age, 1),
        alpha_c = if_else((actual$alpha_c != 0) & (expected$alpha_c != 0), actual$alpha_c / expected$alpha_c, 1),
        basal_area = if_else((actual$basal_area != 0) & (expected$basal_area != 0), actual$basal_area / expected$basal_area, 1),
        basal_area_prop = if_else((actual$basal_area_prop != 0) & (expected$basal_area_prop != 0), actual$basal_area_prop / expected$basal_area_prop, 1),
        biom_foliage = if_else((actual$biom_foliage != 0) & (expected$biom_foliage != 0), actual$biom_foliage / expected$biom_foliage, 1),
        biom_foliage_debt = if_else((actual$biom_foliage_debt != 0) & (expected$biom_foliage_debt != 0), actual$biom_foliage_debt / expected$biom_foliage_debt, 1),
        biom_root = if_else((actual$biom_root != 0) & (expected$biom_root != 0), actual$biom_root / expected$biom_root, 1),
        biom_stem = if_else((actual$biom_stem != 0) & (expected$biom_stem != 0), actual$biom_stem / expected$biom_stem, 1),
        biom_tree = if_else((actual$biom_tree != 0) & (expected$biom_tree != 0), actual$biom_tree / expected$biom_tree, 1),
        biom_tree_max = if_else((actual$biom_tree_max != 0) & (expected$biom_tree_max != 0), actual$biom_tree_max / expected$biom_tree_max, 1),
        canopy_cover = if_else((actual$canopy_cover != 0) & (expected$canopy_cover != 0), actual$canopy_cover / expected$canopy_cover, 1),
        canopy_vol_frac = if_else((actual$canopy_vol_frac != 0) & (expected$canopy_vol_frac != 0), actual$canopy_vol_frac / expected$canopy_vol_frac, 1),
        conduct_canopy = if_else((actual$conduct_canopy != 0) & (expected$conduct_canopy != 0), actual$conduct_canopy / expected$conduct_canopy, 1),
        crown_length = if_else((actual$crown_length != 0) & (expected$crown_length != 0), actual$crown_length / expected$crown_length, 1),
        crown_width = if_else((actual$crown_width != 0) & (expected$crown_width != 0), actual$crown_width / expected$crown_width, 1),
        dbh = if_else((actual$dbh != 0) & (expected$dbh != 0), actual$dbh / expected$dbh, 1),
        epsilon_biom_stem = if_else((actual$epsilon_biom_stem != 0) & (expected$epsilon_biom_stem != 0), actual$epsilon_biom_stem / expected$epsilon_biom_stem, 1),
        epsilon_gpp = if_else((actual$epsilon_gpp != 0) & (expected$epsilon_gpp != 0), actual$epsilon_gpp / expected$epsilon_gpp, 1),
        epsilon_npp = if_else((actual$epsilon_npp != 0) & (expected$epsilon_npp != 0), actual$epsilon_npp / expected$epsilon_npp, 1),
        #fracBB = if_else((actual$fracBB != 0) & (expected$fracBB != 0), actual$fracBB / expected$fracBB, 1),
        fi = if_else((actual$fi != 0) & (expected$fi != 0), actual$fi / expected$fi, 1),
        f_age = if_else((actual$f_age != 0) & (expected$f_age != 0), actual$f_age / expected$f_age, 1),
        f_calpha = if_else((actual$f_calpha != 0) & (expected$f_calpha != 0), actual$f_calpha / expected$f_calpha, 1),
        f_cg = if_else((actual$f_cg != 0) & (expected$f_cg != 0), actual$f_cg / expected$f_cg, 1),
        f_frost = if_else((actual$f_frost != 0) & (expected$f_frost != 0), actual$f_frost / expected$f_frost, 1),
        f_phys = if_else((actual$f_phys != 0) & (expected$f_phys != 0), actual$f_phys / expected$f_phys, 1),
        f_nutr = if_else((actual$f_nutr != 0) & (expected$f_nutr != 0), actual$f_nutr / expected$f_nutr, 1),
        f_sw = if_else((actual$f_sw != 0) & (expected$f_sw != 0), actual$f_sw / expected$f_sw, 1),
        f_tmp = if_else((actual$f_tmp != 0) & (expected$f_tmp != 0), actual$f_tmp / expected$f_tmp, 1),
        f_tmp_gc = if_else((actual$f_tmp_gc != 0) & (expected$f_tmp_gc != 0), actual$f_tmp_gc / expected$f_tmp_gc, 1),
        f_vpd = if_else((actual$f_vpd != 0) & (expected$f_vpd != 0), actual$f_vpd / expected$f_vpd, 1),
        gammaF = if_else((actual$gammaF != 0) & (expected$gammaF != 0), actual$gammaF / expected$gammaF, 1),
        #gammaN = if_else((actual$gammaN != 0) & (expected$gammaN != 0), actual$gammaN / expected$gammaN, 1),
        gpp = if_else((actual$gpp != 0) & (expected$gpp != 0), actual$gpp / expected$gpp, 1),
        height = if_else((actual$height != 0) & (expected$height != 0), actual$height / expected$height, 1),
        lai = if_else((actual$lai != 0) & (expected$lai != 0), actual$lai / expected$lai, 1),
        lai_above = if_else((actual$lai_above != 0) & (expected$lai_above != 0), actual$lai_above / expected$lai_above, 1),
        lai_sa_ratio = if_else((actual$lai_sa_ratio != 0) & (expected$lai_sa_ratio != 0), actual$lai_sa_ratio / expected$lai_sa_ratio, 1),
        lambda_h = if_else((actual$lambda_h != 0) & (expected$lambda_h != 0), actual$lambda_h / expected$lambda_h, 1),
        lambda_v = if_else((actual$lambda_v != 0) & (expected$lambda_v != 0), actual$lambda_v / expected$lambda_v, 1),
        layer_id = if_else((actual$layer_id != 0) & (expected$layer_id != 0), actual$layer_id / expected$layer_id, 1),
        mort_stress = if_else((actual$mort_stress != 0) & (expected$mort_stress != 0), actual$mort_stress / expected$mort_stress, 1),
        mort_thinn = if_else((actual$mort_thinn != 0) & (expected$mort_thinn != 0), actual$mort_thinn / expected$mort_thinn, 1),
        #npp_f = if_else((actual$npp_f != 0) & (expected$npp != 0), actual$npp_f / expected$npp, 1), # r3PG calls npp_f npp in its output, https://github.com/trotsiuk/r3PG/issues/69
        npp_f = if_else((actual$npp_f != 0) & (expected$npp_f != 0), actual$npp_f / expected$npp_f, 1), # r3PG calls npp_f npp in its output, https://github.com/trotsiuk/r3PG/issues/69
        npp_fract_foliage = if_else((actual$npp_fract_foliage != 0) & (expected$npp_fract_foliage != 0), actual$npp_fract_foliage / expected$npp_fract_foliage, 1),
        npp_fract_root = if_else((actual$npp_fract_root != 0) & (expected$npp_fract_root != 0), actual$npp_fract_root / expected$npp_fract_root, 1),
        npp_fract_stem = if_else((actual$npp_fract_stem != 0) & (expected$npp_fract_stem != 0), actual$npp_fract_stem / expected$npp_fract_stem, 1),
        prcp_interc = if_else((actual$prcp_interc != 0) & (expected$prcp_interc != 0), actual$prcp_interc / expected$prcp_interc, 1),
        sla = if_else((actual$sla != 0) & (expected$sla != 0), actual$sla / expected$sla, 1),
        stems_n = if_else((actual$stems_n != 0) & (expected$stems_n != 0), actual$stems_n / expected$stems_n, 1),
        transp_veg = if_else((actual$transp_veg != 0) & (expected$transp_veg != 0), actual$transp_veg / expected$transp_veg, 1),
        volume = if_else((actual$volume != 0) & (expected$volume != 0), actual$volume / expected$volume, 1),
        vpd_sp = if_else((actual$vpd_sp != 0) & (expected$vpd_sp != 0), actual$vpd_sp / expected$vpd_sp, 1),
        asw = if_else((actual$asw != 0) & (expected$asw != 0), actual$asw / expected$asw, 1),
        conduct_soil = if_else((actual$conduct_soil != 0) & (expected$conduct_soil != 0), actual$conduct_soil / expected$conduct_soil, 1),
        evapo_transp = if_else((actual$evapo_transp != 0) & (expected$evapo_transp != 0), actual$evapo_transp / expected$evapo_transp, 1),
        evapotra_soil = if_else((actual$evapotra_soil != 0) & (expected$evapotra_soil != 0), actual$evapotra_soil / expected$evapotra_soil, 1),
        f_transp_scale = if_else((actual$f_transp_scale != 0) & (expected$f_transp_scale != 0), actual$f_transp_scale / expected$f_transp_scale, 1),
        irrig_supl = if_else((actual$irrig_supl != 0) & (expected$irrig_supl != 0), actual$irrig_supl / expected$irrig_supl, 1),
        prcp_runoff = if_else((actual$prcp_runoff != 0) & (expected$prcp_runoff != 0), actual$prcp_runoff / expected$prcp_runoff, 1),
        #volume_cum = if_else((actual$volume_cum != 0) & (expected$volume_cum != 0), actual$volume_cum / expected$volume_cum, 1),
        wood_density = if_else((actual$wood_density != 0) & (expected$wood_density != 0), actual$wood_density / expected$wood_density, 1),
        wue = if_else((actual$wue != 0) & (expected$wue != 0), actual$wue / expected$wue, 1),
        wue_transp = if_else((actual$wue_transp != 0) & (expected$wue_transp != 0), actual$wue_transp / expected$wue_transp,  1))
    if (("CVdbhDistribution" %in% actualColumns) & ("CVdbhDistribution" %in% expectedColumns))
    {
      ratios %<>%
        mutate(CVdbhDistribution = if_else((actual$CVdbhDistribution != 0) & (expected$CVdbhDistribution != 0), actual$CVdbhDistribution / expected$CVdbhDistribution, 1),
               CVwsDistribution = if_else((actual$CVwsDistribution != 0) & (expected$CVwsDistribution != 0), actual$CVwsDistribution / expected$CVwsDistribution, 1),
               height_rel = if_else((actual$height_rel != 0) & (expected$height_rel != 0), actual$height_rel / expected$height_rel, 1),
               DWeibullScale = if_else((actual$DWeibullScale != 0) & (expected$DWeibullScale != 0), actual$DWeibullScale / expected$DWeibullScale, 1),
               DWeibullShape = if_else((actual$DWeibullShape != 0) & (expected$DWeibullShape != 0), actual$DWeibullShape / expected$DWeibullShape, 1),
               DWeibullLocation = if_else((actual$DWeibullLocation != 0) & (expected$DWeibullLocation != 0), actual$DWeibullLocation / expected$DWeibullLocation, 1),
               wsWeibullScale = if_else((actual$wsWeibullScale != 0) & (expected$wsWeibullScale != 0), actual$wsWeibullScale / expected$wsWeibullScale, 1),
               wsWeibullShape = if_else((actual$wsWeibullShape != 0) & (expected$wsWeibullShape != 0), actual$wsWeibullShape / expected$wsWeibullShape, 1),
               wsWeibullLocation = if_else((actual$wsWeibullLocation != 0) & (expected$wsWeibullLocation != 0), actual$wsWeibullLocation / expected$wsWeibullLocation, 1),
               #DWeibullScale = if_else((actual$DWeibullScale != 0) & (expected$Dweibullscale != 0), actual$DWeibullScale / expected$Dweibullscale, 1),
               #DWeibullShape = if_else((actual$DWeibullShape != 0) & (expected$Dweibullshape != 0), actual$DWeibullShape / expected$Dweibullshape, 1),
               #DWeibullLocation = if_else((actual$DWeibullLocation != 0) & (expected$Dweibulllocation != 0), actual$DWeibullLocation / expected$Dweibulllocation, 1),
               #wsWeibullScale = if_else((actual$wsWeibullScale != 0) & (expected$wsweibullscale != 0), actual$wsWeibullScale / expected$wsweibullscale, 1),
               #wsWeibullShape = if_else((actual$wsWeibullShape != 0) & (expected$wsweibullshape != 0), actual$wsWeibullShape / expected$wsweibullshape, 1),
               #wsWeibullLocation = if_else((actual$wsWeibullLocation != 0) & (expected$wsweibulllocation != 0), actual$wsWeibullLocation / expected$wsweibulllocation, 1),
               DrelBiaspFS = if_else((actual$DrelBiaspFS != 0) & (expected$DrelBiaspFS != 0), actual$DrelBiaspFS / expected$DrelBiaspFS, 1),
               DrelBiasheight = if_else((actual$DrelBiasheight != 0) & (expected$DrelBiasheight != 0), actual$DrelBiasheight / expected$DrelBiasheight, 1),
               DrelBiasBasArea = if_else((actual$DrelBiasBasArea != 0) & (expected$DrelBiasBasArea != 0), actual$DrelBiasBasArea / expected$DrelBiasBasArea, 1),
               DrelBiasLCL = if_else((actual$DrelBiasLCL != 0) & (expected$DrelBiasLCL != 0), actual$DrelBiasLCL / expected$DrelBiasLCL, 1),
               DrelBiasCrowndiameter = if_else((actual$DrelBiasCrowndiameter != 0) & (expected$DrelBiasCrowndiameter != 0), actual$DrelBiasCrowndiameter / expected$DrelBiasCrowndiameter, 1),
               wsrelBias = if_else((actual$wsrelBias != 0) & (expected$wsrelBias != 0), actual$wsrelBias / expected$wsrelBias, 1))
    }
    ratios %<>% slice(2:n()) # exclude first row as it's not fully populated
  }
  return(ratios)
}

plot_departures = function(ratios, fillLimits = NULL, legend = TRUE, title = NULL, xLabel = TRUE, yLabels = TRUE)
{
  ratiosForRaster = ratios %>% select(-species, -age) %>%
    mutate(timestep = row_number()) %>% 
    pivot_longer(!timestep, values_to = "ratio")

  xAxisLabel = NULL
  if (xLabel)
  {
    xAxisLabel = "timestep"
  }
  plot = ggplot(ratiosForRaster) +
    geom_raster(aes(x = timestep, y = name, fill = ratio)) +
    labs(title = title, x = xAxisLabel, y = NULL, fill = "ratio")
  if (is.null(fillLimits))
  {
    plot = plot +
      scale_fill_viridis_c(trans = "log10")
  }
  else
  {
    plot = plot +
      scale_fill_viridis_c(trans = "log10", limits = fillLimits)
  }
  if (yLabels == FALSE)
  {
    plot = plot +
      scale_y_discrete(labels = NULL, limits = rev)
  }
  else
  {
    plot = plot +
      scale_y_discrete(limits = rev)
  }
  if (legend == FALSE)
  {
    plot = plot +
      theme(legend.position = "none")
  }
  
  return(plot)
}

read_actual = function(spreadsheet)
{
  columnTypes = cols("date" = col_date(format = "%Y-%M"), "species" = col_character(), .default = col_double())
  actual = read_csv(file.path(getwd(), "TestResults", spreadsheet), col_types = columnTypes)
  return(actual)
}

read_expected = function(worksheet)
{
  expected = read_xlsx(file.path(getwd(), "UnitTests/r3PG validation stands.xlsx"), worksheet)
  return(expected)
}

summarize_ratios = function(ratios)
{
  columns = colnames(ratios)
  
  quantiles = c(0, 0.5, 1)
  summary = ratios %>% 
    group_by(species) %>%
    summarize(quantile = quantiles,
              aero_resist = quantile(aero_resist, probs = quantiles, na.rm = TRUE),
              alpha_c = quantile(alpha_c, probs = quantiles, na.rm = TRUE),
              basal_area = quantile(basal_area, probs = quantiles),
              basal_area_prop = quantile(basal_area_prop, probs = quantiles),
              biom_foliage = quantile(biom_foliage, probs = quantiles, na.rm = TRUE),
              biom_foliage_debt = quantile(biom_foliage_debt, probs = quantiles, na.rm = TRUE),
              biom_root = quantile(biom_root, probs = quantiles),
              biom_stem = quantile(biom_stem, probs = quantiles),
              biom_tree = quantile(biom_tree, probs = quantiles),
              biom_tree_max = quantile(biom_tree_max, probs = quantiles),
              canopy_cover = quantile(canopy_cover, probs = quantiles, na.rm = TRUE),
              canopy_vol_frac = quantile(canopy_vol_frac, probs = quantiles, na.rm = TRUE),
              conduct_canopy = quantile(conduct_canopy, probs = quantiles, na.rm = TRUE),
              crown_length = quantile(crown_length, probs = quantiles, na.rm = TRUE),
              crown_width = quantile(crown_width, probs = quantiles, na.rm = TRUE),
              dbh = quantile(dbh, probs = quantiles, na.rm = TRUE),
              epsilon_biom_stem = quantile(epsilon_gpp, probs = quantiles, na.rm = TRUE),
              epsilon_gpp = quantile(epsilon_gpp, probs = quantiles, na.rm = TRUE),
              epsilon_npp = quantile(epsilon_npp, probs = quantiles, na.rm = TRUE),
              f_age = quantile(f_age, probs = quantiles),
              f_calpha = quantile(f_calpha, probs = quantiles),
              f_cg = quantile(f_cg, probs = quantiles),
              f_frost = quantile(f_frost, probs = quantiles, na.rm = TRUE),
              f_phys = quantile(f_phys, probs = quantiles, na.rm = TRUE),
              f_nutr = quantile(f_nutr, probs = quantiles, na.rm = TRUE),
              f_sw = quantile(f_sw, probs = quantiles, na.rm = TRUE),
              f_tmp = quantile(f_tmp, probs = quantiles),
              f_tmp_gc = quantile(f_tmp_gc, probs = quantiles, na.rm = TRUE),
              f_vpd = quantile(f_vpd, probs = quantiles, na.rm = TRUE),
              fi = quantile(fi, probs = quantiles, na.rm = TRUE),
              #gC = quantile(gC, probs = quantiles, na.rm = TRUE),
              gammaF = quantile(gammaF, probs = quantiles, na.rm = TRUE),
              #gammaN = quantile(gammaN, probs = quantiles, na.rm = TRUE),
              gpp = quantile(gpp, probs = quantiles, na.rm = TRUE),
              height = quantile(height, probs = quantiles),
              lai = quantile(lai, probs = quantiles, na.rm = TRUE),
              lai_above = quantile(lai_above, probs = quantiles, na.rm = TRUE),
              lai_sa_ratio = quantile(lai_sa_ratio, probs = quantiles, na.rm = TRUE),
              lambda_h = quantile(lambda_h, probs = quantiles, na.rm = TRUE),
              lambda_v = quantile(lambda_v, probs = quantiles, na.rm = TRUE),
              layer_id = quantile(layer_id, probs = quantiles, na.rm = TRUE),
              mort_stress = quantile(mort_stress, probs = quantiles),
              mort_thinn = quantile(mort_thinn, probs = quantiles),
              npp_f = quantile(npp_f, probs = quantiles, na.rm = TRUE),
              npp_fract_foliage = quantile(npp_fract_foliage, probs = quantiles, na.rm = TRUE),
              npp_fract_root = quantile(npp_fract_root, probs = quantiles, na.rm = TRUE),
              npp_fract_stem = quantile(npp_fract_stem, probs = quantiles, na.rm = TRUE),
              prcp_interc = quantile(prcp_interc, probs = quantiles, na.rm = TRUE),
              sla = quantile(sla, probs = quantiles, na.rm = TRUE),
              stems_n = quantile(stems_n, probs = quantiles),
              transp_veg = quantile(transp_veg, probs = quantiles),
              volume = quantile(volume, probs = quantiles),
              vpd_sp = quantile(vpd_sp, probs = quantiles, na.rm = TRUE),
              asw = quantile(asw, probs = quantiles, na.rm = TRUE), 
              conduct_soil = quantile(conduct_soil, probs = quantiles, na.rm = TRUE), 
              evapotra_soil = quantile(evapotra_soil, probs = quantiles, na.rm = TRUE), 
              f_transp_scale = quantile(f_transp_scale, probs = quantiles, na.rm = TRUE), 
              irrig_supl = quantile(irrig_supl, probs = quantiles, na.rm = TRUE), 
              prcp_runoff = quantile(prcp_runoff, probs = quantiles, na.rm = TRUE), 
              .groups = "drop")
  if ("CVdbhDistribution" %in% columns)
  {
    biasSummary = ratios %>% 
      group_by(species) %>%
      summarize(quantile = quantiles,
                CVdbhDistribution = quantile(CVdbhDistribution, probs = quantiles, na.rm = TRUE), 
                CVwsDistribution = quantile(CVwsDistribution, probs = quantiles, na.rm = TRUE), 
                height_rel = quantile(height_rel, probs = quantiles, na.rm = TRUE), 
                DWeibullScale = quantile(DWeibullScale, probs = quantiles, na.rm = TRUE), 
                DWeibullShape = quantile(DWeibullShape, probs = quantiles, na.rm = TRUE), 
                DWeibullLocation = quantile(DWeibullLocation, probs = quantiles, na.rm = TRUE), 
                wsWeibullScale = quantile(wsWeibullScale, probs = quantiles, na.rm = TRUE), 
                wsWeibullShape = quantile(wsWeibullShape, probs = quantiles, na.rm = TRUE), 
                wsWeibullLocation = quantile(wsWeibullLocation, probs = quantiles, na.rm = TRUE), 
                DrelBiaspFS = quantile(DrelBiaspFS, probs = quantiles, na.rm = TRUE), 
                DrelBiasheight = quantile(DrelBiasheight, probs = quantiles, na.rm = TRUE), 
                DrelBiasBasArea = quantile(DrelBiasBasArea, probs = quantiles, na.rm = TRUE), 
                DrelBiasLCL = quantile(DrelBiasLCL, probs = quantiles, na.rm = TRUE), 
                DrelBiasCrowndiameter = quantile(DrelBiasCrowndiameter, probs = quantiles, na.rm = TRUE), 
                wsrelBias = quantile(wsrelBias, probs = quantiles, na.rm = TRUE),
                .groups = "drop")
    summary = bind_rows(summary, biasSummary)
  }
  return(summary)
}

