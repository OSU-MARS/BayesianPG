library(dplyr)
library(sensobol)
library(readxl)
library(writexl)

## Sobol matrix generation for sensitivity analysis
# setup: for now, evergreen species without δ13C calculations are assumed
# For now, these columns are excluded:
#   crownshape: not amenable to Sobol' indexing as a factorial variable (also 3-PGmix specific)
#   leafgrow, leaffall: leaf on and off not applicable to evergreen species
#   aV, nVB, nVH, nVBH: not meaningful for sensitivity analysis if merchantable m³ weren't measured
#   fulCanAge: not used by 3-PGmix
#   fracBB0, fracBB1, tBB: not used for calculating merchantable m³ when aV ≠ 0
#   nHLL and nHLC: not used for crown length with exponent height models
#   RGcGw, D13CTissueDif, aFracDiffu, bFracRubi: δ13C parameters have no effect when δ13C is disabled
parameterPriors = read_xlsx("UnitTests/sensitivity analysis.xlsx", sheet = "parameter priors")

sobolSpecies = "PSME" # TSHE, THPL, ALRU2, (PISI)
sobolN = 1 # small values for unit test generation, N >= 1000 likely required for accurate estimation of Sobol' indices
sobolOrder = "first"

defaults = parameterPriors %>% filter(species == sobolSpecies)
maxes = parameterPriors %>% filter(species == paste(sobolSpecies, "max")) %>% 
  select(-species, -crownshape, -leafgrow, -leaffall, -aV, -nVB, -nVH, -nVBH, -RGcGw, -D13CTissueDif, -aFracDiffu, -bFracRubi)
mins = parameterPriors %>% filter(species == paste(sobolSpecies, "min")) %>% 
  select(-species, -crownshape, -leafgrow, -leaffall, -aV, -nVB, -nVH, -nVBH, -RGcGw, -D13CTissueDif, -aFracDiffu, -bFracRubi)
ranges = maxes - mins
sobolParameters = colnames(ranges)

# generate Sobol' samples and translate back to parameter space
parameterizations = bind_cols(species = sobolSpecies,
                              sobol_matrices(N = sobolN, order = sobolOrder, params = sobolParameters),
                              crownshape = defaults$crownshape,
                              leafgrow = defaults$leafgrow,
                              leaffall = defaults$leaffall) %>%
  mutate(parameterization = row_number(),
         pFS2 = mins$pFS2 + ranges$pFS2 * pFS2,
         pFS20 = mins$pFS20 + ranges$pFS20 * pFS20,
         aWS = mins$aWS + ranges$aWS * aWS,
         nWS = mins$nWS + ranges$nWS * nWS,
         pRx = mins$pRx + ranges$pRx * pRx,
         pRn = mins$pRn + ranges$pRn * pRn,
         gammaF1 = mins$gammaF1 + ranges$gammaF1 * gammaF1,
         gammaF0 = mins$gammaF0 + ranges$gammaF0 * gammaF0,
         tgammaF = mins$tgammaF + ranges$tgammaF * tgammaF,
         gammaR = mins$gammaR + ranges$gammaR * gammaR,
         Tmin = mins$Tmin + ranges$Tmin * Tmin,
         Topt = mins$Topt + ranges$Topt * Topt,
         Tmax = mins$Tmax + ranges$Tmax * Tmax,
         kF = mins$kF + ranges$kF * kF,
         SWconst = mins$SWconst + ranges$SWconst * SWconst,
         SWpower = mins$SWpower + ranges$SWpower * SWpower,
         fCalpha700 = mins$fCalpha700 + ranges$fCalpha700 * fCalpha700,
         fCg700 = mins$fCg700 + ranges$fCg700 * fCg700,
         m0 = mins$m0 + ranges$m0 * m0,
         fN0 = mins$fN0 + ranges$fN0 * fN0,
         fNn = mins$fNn + ranges$fNn * fNn,
         MaxAge = mins$MaxAge + ranges$MaxAge * MaxAge,
         nAge = mins$nAge + ranges$nAge * nAge,
         rAge = mins$rAge + ranges$rAge * rAge,
         gammaN1 = mins$gammaN1 + ranges$gammaN1 * gammaN1,
         gammaN0 = mins$gammaN0 + ranges$gammaN0 * gammaN0,
         tgammaN = mins$tgammaN + ranges$tgammaN * tgammaN,
         ngammaN = mins$ngammaN + ranges$ngammaN * ngammaN,
         wSx1000 = mins$wSx1000 + ranges$wSx1000 * wSx1000,
         thinPower = mins$thinPower + ranges$thinPower * thinPower,
         mF = mins$mF + ranges$mF * mF,
         mR = mins$mR + ranges$mR * mR,
         mS = mins$mS + ranges$mS * mS,
         SLA0 = mins$SLA0 + ranges$SLA0 * SLA0,
         SLA1 = mins$SLA1 + ranges$SLA1 * SLA1,
         tSLA = mins$tSLA + ranges$tSLA * tSLA,
         k = mins$k + ranges$k * k,
         fullCanAge = mins$fullCanAge + ranges$fullCanAge * fullCanAge,
         MaxIntcptn = mins$MaxIntcptn + ranges$MaxIntcptn * MaxIntcptn,
         LAImaxIntcptn = mins$LAImaxIntcptn + ranges$LAImaxIntcptn * LAImaxIntcptn,
         cVPD = mins$cVPD + ranges$cVPD * cVPD,
         alphaCx = mins$alphaCx + ranges$alphaCx * alphaCx,
         Y = mins$Y + ranges$Y * Y,
         MinCond = mins$MinCond + ranges$MinCond * MinCond,
         MaxCond = mins$MaxCond + ranges$MaxCond * MaxCond,
         LAIgcx = mins$LAIgcx + ranges$LAIgcx * LAIgcx,
         CoeffCond = mins$CoeffCond + ranges$CoeffCond * CoeffCond,
         BLcond = mins$BLcond + ranges$BLcond * BLcond,
         fracBB0 = mins$fracBB0 + ranges$fracBB0 * fracBB0,
         fracBB1 = mins$fracBB1 + ranges$fracBB1 * fracBB1,
         tBB = mins$tBB + ranges$tBB * tBB,
         rhoMin = mins$rhoMin + ranges$rhoMin * rhoMin,
         rhoMax = mins$rhoMax + ranges$rhoMax * rhoMax,
         tRho = mins$tRho + ranges$tRho * tRho,
         aH = mins$aH + ranges$aH * aH,
         nHB = mins$nHB + ranges$nHB * nHB,
         nHC = mins$nHC + ranges$nHC * nHC,
         aV = mins$aV + ranges$aV * aV,
         nVB = mins$nVB + ranges$nVB * nVB,
         nVH = mins$nVH + ranges$nVH * nVH,
         nVBH = mins$nVBH + ranges$nVBH * nVBH,
         aK = mins$aK + ranges$aK * aK,
         nKB = mins$nKB + ranges$nKB * nKB,
         nKH = mins$nKH + ranges$nKH * nKH,
         nKC = mins$nKC + ranges$nKC * nKC,
         nKrh = mins$nKrh + ranges$nKrh * nKrh,
         aHL = mins$aHL + ranges$aHL * aHL,
         nHLB = mins$nHLB + ranges$nHLB * nHLB,
         nHLL = mins$nHLL + ranges$nHLL * nHLL,
         nHLC = mins$nHLC + ranges$nHLC * nHLC,
         nHLrh = mins$nHLrh + ranges$nHLrh * nHLrh,
         Qa = mins$Qa + ranges$Qa * Qa,
         Qb = mins$Qb + ranges$Qb * Qb,
         gDM_mol = mins$gDM_mol + ranges$gDM_mol * gDM_mol,
         molPAR_MJ = mins$molPAR_MJ + ranges$molPAR_MJ * molPAR_MJ) %>%
  relocate(species, parameterization)
#write_xlsx(parameterizations, paste0("UnitTests/", sobolSpecies, " parameterizations Sobol ", sobolOrder, " order N = ", sobolN, " ", nrow(parameterizations), ".xlsx"))
