-- select * from FONDSUSER.FON_FONDSEN t where t.fondsid in (3829,456487,2147695,1319887,1495636)
select t.fondsid, t.val_valutacode, t.fondsnaam, t.symbool, t.cfi_code  from FONDSUSER.FON_FONDSEN t where t.fondsnaam like '%Morningstar%' and t.hft_hoofdfondstype_nr=1 --.hft_hoofdfondstype_nr not in (1,)
--select t.hft_hoofdfondstype_nr, t.fst_fondstype_nr, t.fondsid, t.val_valutacode, t.fondsnaam, t.symbool, t.cfi_code  from FONDSUSER.FON_FONDSEN t where t.fondsnaam like '%Barclays%'


select t.quote_datetime, t.price from QDS.QDS_MANUAL_QUOTES t where t.fon_security_id=2919561 and t.kty_quote_type_nr=3 order by t.quote_datetime asc

select * from FONDSUSER.FON_FONDSSUBTYPES t