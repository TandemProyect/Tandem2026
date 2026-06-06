 (DEFUN C:Juan26 ()
 (setq cmdecho (getvar "CMDECHO"))
	(setvar "CMDECHO" 0)
	(setq nMater 1)
(setq list_of_files (list                                                           
"017010s.dwg" 
"017020s.dwg" 
"019780s.dwg" 
"026411s.dwg" 
"026412s.dwg" 
"026413s.dwg" 
"026414s.dwg" 
"100000s.dwg" 
"100003s.dwg" 
"100005s.dwg" 
"100007s.dwg" 
"100009s.dwg" 
"100012s.dwg" 
"100013s.dwg" 
"100014s.dwg" 
"100017s.dwg" 
"100021s.dwg" 
"100023s.dwg" 
"100025s.dwg" 
"100027s.dwg" 
"100029s.dwg" 
"100055s.dwg" 
"100057s.dwg" 
"100059s.dwg" 
"100061s.dwg" 
"100063s.dwg" 
"100065s.dwg" 
"100067s.dwg" 
"100069s.dwg" 
"100088s.dwg" 
"100091s.dwg" 
"100093s.dwg" 
"100159s.dwg" 
"100183s.dwg" 
"100185s.dwg" 
"100242s.dwg" 
"100244s.dwg" 
"100330s.dwg" 
"100336s.dwg" 
"100339s.dwg" 
"100411s.dwg" 
"100529s.dwg" 
"100572s.dwg" 
"100573s.dwg" 
"100574s.dwg" 
"100575s.dwg" 
"100742s.dwg" 
"100830s.dwg" 
"100852s.dwg" 
"100863s.dwg" 
"100908s.dwg" 
"100973s.dwg" 
"100981s.dwg" 
"100985s.dwg" 
"100989s.dwg" 
"100993s.dwg" 
"101159s.dwg" 
"101306s.dwg" 
"101309s.dwg" 
"101656s.dwg" 
"101657s.dwg" 
"101658s.dwg" 
"101659s.dwg" 
"101731s.dwg" 
"102400s.dwg" 
"102846s.dwg" 
"102859s.dwg" 
"102860s.dwg" 
"102861s.dwg" 
"102862s.dwg" 
"102951s.dwg" 
"102954s.dwg" 
"102957s.dwg" 
"104029s.dwg" 
"104391s.dwg" 
"104762s.dwg" 
"104766s.dwg" 
"104779s.dwg" 
"104780s.dwg" 
"105824s.dwg" 
"105925s.dwg" 
"106092s.dwg" 
"106624s.dwg" 
"106630s.dwg" 
"106880s.dwg" 
"107002s.dwg" 
"107867s.dwg" 
"108380s.dwg" 
"108540s.dwg" 
"108689s.dwg" 
"109034s.dwg" 
"109107s.dwg" 
"109108s.dwg" 
"109109s.dwg" 
"109110s.dwg" 
"109198s.dwg" 
"109208s.dwg" 
"109219s.dwg" 
"109755s.dwg" 
"109764s.dwg" 
"109783s.dwg" 
"109879s.dwg" 
"110073s.dwg" 
"110160s.dwg" 
"110176s.dwg" 
"110208s.dwg" 
"110211s.dwg" 
"110213s.dwg" 
"110347s.dwg" 
"110514s.dwg" 
"110792s.dwg" 
"110793s.dwg" 
"111087s.dwg" 
"111095s.dwg" 
"111101s.dwg" 
"111103s.dwg" 
"111117s.dwg" 
"111124s.dwg" 
"112676s.dwg" 
"112678s.dwg" 
"112765s.dwg" 
"112809s.dwg" 
"112926s.dwg" 
"113228s.dwg" 
"113358s.dwg" 
"114148s.dwg" 
"114179s.dwg" 
"114180s.dwg" 
"114287s.dwg" 
"114536s.dwg" 
"114595s.dwg" 
"114613s.dwg" 
"114629s.dwg" 
"114632s.dwg" 
"114638s.dwg" 
"114641s.dwg" 
"114645s.dwg" 
"114648s.dwg" 
"114651s.dwg" 
"114654s.dwg" 
"114681s.dwg" 
"114687s.dwg" 
"114691s.dwg" 
"114695s.dwg" 
"114818s.dwg" 
"114819s.dwg" 
"114904s.dwg" 
"114996s.dwg" 
"115156s.dwg" 
"115157s.dwg" 
"115189s.dwg" 
"115354s.dwg" 
"115513s.dwg" 
"115959s.dwg" 
"116356s.dwg" 
"116762s.dwg" 
"117032s.dwg" 
"117194s.dwg" 
"117689s.dwg" 
"123771s.dwg" 
"124109s.dwg" 
"124112s.dwg" 
"124115s.dwg" 
"124118s.dwg" 
"124121s.dwg" 
"124124s.dwg" 
"124170s.dwg" 
"124558s.dwg" 
"124561s.dwg" 
"124915s.dwg" 
"125672s.dwg" 
"125976s.dwg" 
"126314s.dwg" 
"126318s.dwg" 
"126392s.dwg" 
"126393s.dwg" 
"126453s.dwg" 
"126675s.dwg" 
"127537s.dwg" 
"129272s.dwg" 
"129490s.dwg" 
"129492s.dwg" 
"129494s.dwg" 
"129496s.dwg" 
"129498s.dwg" 
"129500s.dwg" 
"129502s.dwg" 
"129504s.dwg" 
"129947s.dwg" 
"129982s.dwg" 
"130009s.dwg" 
"130162s.dwg" 
"130171s.dwg" 
"130180s.dwg" 
"130189s.dwg" 
"130191s.dwg" 
"130193s.dwg" 
"130195s.dwg" 
"130197s.dwg" 
"130199s.dwg" 
"130201s.dwg" 
"130225s.dwg" 
"130228s.dwg" 
"130233s.dwg" 
"130282s.dwg" 
"130283s.dwg" 
"130309s.dwg" 
"130313s.dwg" 
"130317s.dwg" 
"130321s.dwg" 
"130365s.dwg" 
"130372s.dwg" 
"130378s.dwg" 
"130390s.dwg" 
"130425s.dwg" 
"130426s.dwg" 
"130431s.dwg" 
"130438s.dwg" 
"130441s.dwg" 
"130445s.dwg" 
"130448s.dwg" 
"130450s.dwg" 
"130476s.dwg" 
"130481s.dwg" 
"130512s.dwg" 
"130583s.dwg" 
"130619s.dwg" 
"130621s.dwg" 
"130681s.dwg" 
"130684s.dwg" 
"131368s.dwg" 
"131788s.dwg" 
"131789s.dwg" 
"131790s.dwg" 
"131791s.dwg" 
"131863s.dwg" 
"131995s.dwg" 
"131998s.dwg" 
"132004s.dwg" 
"132007s.dwg" 
"132010s.dwg" 
"132013s.dwg" 
"132016s.dwg" 
"132019s.dwg" 
"132022s.dwg" 
"132025s.dwg" 
"132123s.dwg" 
"132194s.dwg" 
"132198s.dwg" 
"132200s.dwg" 
"132202s.dwg" 
"132213s.dwg" 
"132219s.dwg" 
"132224s.dwg" 
"132229s.dwg" 
"132234s.dwg" 
"132239s.dwg" 
"132479s.dwg" 
"132488s.dwg" 
"132492s.dwg" 
"132502s.dwg" 
"132505s.dwg" 
"132508s.dwg" 
"132511s.dwg" 
"132515s.dwg" 
"132592s.dwg" 
"132771s.dwg" 
"132775s.dwg" 
"132777s.dwg" 
"132779s.dwg" 
"132781s.dwg" 
"132783s.dwg" 
"132785s.dwg" 
"132787s.dwg" 
"132789s.dwg" 
"132791s.dwg" 
"132793s.dwg" 
"132795s.dwg" 
"132797s.dwg" 
"132808s.dwg" 
"132810s.dwg" 
"132812s.dwg" 
"132814s.dwg" 
"132816s.dwg" 
"132827s.dwg" 
"132829s.dwg" 
"132831s.dwg" 
"132833s.dwg" 
"132882s.dwg" 
"132928s.dwg" 
"133285s.dwg" 
"133286s.dwg" 
"133287s.dwg" 
"133289s.dwg" 
"133418s.dwg" 
"133421s.dwg" 
"133424s.dwg" 
"133427s.dwg" 
"133430s.dwg" 
"133433s.dwg" 
"133436s.dwg" 
"133439s.dwg" 
"133442s.dwg" 
"133445s.dwg" 
"133448s.dwg" 
"133451s.dwg" 
"133454s.dwg" 
"133457s.dwg" 
"133463s.dwg" 
"133492s.dwg" 
"133499s.dwg" 
"133521s.dwg" 
"133522s.dwg" 
"133523s.dwg" 
"133524s.dwg" 
"133525s.dwg" 
"133543s.dwg" 
"133705s.dwg" 
"133903s.dwg" 
"134005s.dwg" 
"134100s.dwg" 
"134104s.dwg" 
"134512s.dwg" 
"134520s.dwg" 
"134537s.dwg" 
"134539s.dwg" 
"134542s.dwg" 
"134552s.dwg" 
"134553s.dwg" 
"134554s.dwg" 
"134556s.dwg" 
"134557s.dwg" 
"134558s.dwg" 
"134561s.dwg" 
"134562s.dwg" 
"134563s.dwg" 
"134564s.dwg" 
"134628s.dwg" 
"135026s.dwg" 
"135027s.dwg" 
"135187s.dwg" 
"135365s.dwg" 
"135367s.dwg" 
"135368s.dwg" 
"135369s.dwg" 
"135374s.dwg" 
"135375s.dwg" 
"135376s.dwg" 
"135972s.dwg" 
"136582s.dwg" 
"137020s.dwg" 
"137025s.dwg" 
"137030s.dwg" 
"137035s.dwg" 
"137509s.dwg" 
"137514s.dwg" 
"137517s.dwg" 
"137595s.dwg" 
"137911s.dwg" 
"138470s.dwg" 
"138473s.dwg" 
"138490s.dwg" 
"138499_inners.dwg" 
"138499_outers.dwg" 
"138506_inners.dwg" 
"138506_outers.dwg" 
"138532s.dwg" 
"138533s.dwg" 
"138534s.dwg" 
"138535s.dwg" 
"138536s.dwg" 
"138537s.dwg" 
"138538s.dwg" 
"138539s.dwg" 
"138603s.dwg" 
"220002s.dwg" 
"220003s.dwg" 
"220004s.dwg" 
"220005s.dwg" 
"220006s.dwg" 
"220008s.dwg" 
"220010s.dwg" 
"220012s.dwg" 
"220014s.dwg" 
"220016s.dwg" 
"220020s.dwg" 
"220525s.dwg" 
"220601s.dwg" 
"220603s.dwg" 
"234250s.dwg" 
"234300s.dwg" 
"234400s.dwg" 
"234500s.dwg" 
"234600s.dwg" 
"Mdsl32s.dwg" 
"Mdsl50s.dwg" 
"Mdsl70s.dwg" 
"tube50s.dwg" 
"tube75s.dwg" 
))
(foreach e list_of_files
(setq ibloque e)
(c:DWGGenerateElementRevitFamilyr)
)
(setvar "CMDECHO" cmdecho)  
)

(DEFUN C:DWGGenerateElementRevitFamilyr()
(setq nMater(+ nMater  1)) 
 (print nMater)
 (print ibloque)
 (setq btoInsert (strcat "C:/Temp/Revit_2026/s/" ibloque))
 (setq Master (strcat "Master_"  (itoa nMater)))
 (print Master)
 (print btoInsert)
 (setq iwblock ibloque)
 (setq iwblock2(vl-string-subst "b" "r" iwblock))
 (print iwblock2)
 (setq bltoCreate (strcat "C:/Temp/Revit_2026/s/r/" iwblock2))
 (print bltoCreate)
(command "_.insert" btoInsert "0,0" "1.0" "1.0" "")
(command "._explode" (cdr (assoc -1 (entget (entlast)))))
(c:ExplodeExplodableBlockr)
  (setvar "expert" 1)
  (command "_.explode" (ssget "_X") "")
  (command "_.explode" (ssget "_X") "")
  (print "Final explode")
  (setq SSetEblock (ssget "_P"))
  (command "._block" Master "0,0" "_ALL" "")
 (command "_.wblock" bltoCreate Master)
 (print Master)
 ;;(setvar "CMDECHO" 0)
 (command "_.layer" "_UNL" "*" "")
 (command "_.layer" "_t" "*" "")
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (command "._erase" "_all" "" "")
 ;; (setvar "expert" expert)
) 
(DEFUN C:ExplodeExplodableBlockr ()
 (vlax-for b (vla-get-blocks (vla-get-activedocument (vlax-get-acad-object)))
    (if	(= 0 (vlax-get b 'islayout) (vlax-get b 'isxref))
      (vlax-put b 'explodable -1)
    )
  )
  ) 





(DEFUN C:juan2 ()
  (setq block-name (getstring "\nEnter block name: "))
  (setq block-ref (ssget (list (cons 0 "INSERT") (cons 2 block-name))))
  (if block-ref
    (progn
      (setq block-ref-ent (entget (car (ssname block-ref 0))))
      (command "_wblock" (strcat block-name ".dwg") block-ref-ent)
    )
    (princ "\nBlock not found.")
  )
  (princ "OK")
)

(DEFUN C:juan55 ()
  	(setq cmdecho (getvar "CMDECHO"))
	(setvar "CMDECHO" 0)
	(setq nMater 1)
(setq list_of_files (list                                                           
"017010s.dwg" 
))
(foreach e list_of_files
(setq ibloque e)
(c:DWGGenerateElementRevitFamily)
)
(setvar "CMDECHO" cmdecho)  
)

(DEFUN C:DWGGenerateRevitFamily ()
 (setq cmdecho (getvar "CMDECHO"))
	(setvar "CMDECHO" 0)
	(setq nMater 1)
(setq list_of_files (list                                                           
"017010s.dwg" 
"017020s.dwg" 
"019780s.dwg" 
"026411s.dwg" 
"026412s.dwg" 
"026413s.dwg" 
"026414s.dwg" 
"100000s.dwg" 
"100003s.dwg" 
"100005s.dwg" 
"100007s.dwg" 
"100009s.dwg" 
"100012s.dwg" 
"100013s.dwg" 
"100014s.dwg" 
"100017s.dwg" 
"100021s.dwg" 
"100023s.dwg" 
"100025s.dwg" 
"100027s.dwg" 
"100029s.dwg" 
"100055s.dwg" 
"100057s.dwg" 
"100059s.dwg" 
"100061s.dwg" 
"100063s.dwg" 
"100065s.dwg" 
"100067s.dwg" 
"100069s.dwg" 
"100088s.dwg" 
"100091s.dwg" 
"100093s.dwg" 
"100159s.dwg" 
"100183s.dwg" 
"100185s.dwg" 
"100242s.dwg" 
"100244s.dwg" 
"100330s.dwg" 
"100336s.dwg" 
"100339s.dwg" 
"100411s.dwg" 
"100529s.dwg" 
"100572s.dwg" 
"100573s.dwg" 
"100574s.dwg" 
"100575s.dwg" 
"100742s.dwg" 
"100830s.dwg" 
"100852s.dwg" 
"100863s.dwg" 
"100908s.dwg" 
"100973s.dwg" 
"100981s.dwg" 
"100985s.dwg" 
"100989s.dwg" 
"100993s.dwg" 
"101159s.dwg" 
"101306s.dwg" 
"101309s.dwg" 
"101656s.dwg" 
"101657s.dwg" 
"101658s.dwg" 
"101659s.dwg" 
"101731s.dwg" 
"102400s.dwg" 
"102846s.dwg" 
"102859s.dwg" 
"102860s.dwg" 
"102861s.dwg" 
"102862s.dwg" 
"102951s.dwg" 
"102954s.dwg" 
"102957s.dwg" 
"104029s.dwg" 
"104391s.dwg" 
"104762s.dwg" 
"104766s.dwg" 
"104779s.dwg" 
"104780s.dwg" 
"105824s.dwg" 
"105925s.dwg" 
"106092s.dwg" 
"106624s.dwg" 
"106630s.dwg" 
"106880s.dwg" 
"107002s.dwg" 
"107867s.dwg" 
"108380s.dwg" 
"108540s.dwg" 
"108689s.dwg" 
"109034s.dwg" 
"109107s.dwg" 
"109108s.dwg" 
"109109s.dwg" 
"109110s.dwg" 
"109198s.dwg" 
"109208s.dwg" 
"109219s.dwg" 
"109755s.dwg" 
"109764s.dwg" 
"109783s.dwg" 
"109879s.dwg" 
"110073s.dwg" 
"110160s.dwg" 
"110176s.dwg" 
"110208s.dwg" 
"110211s.dwg" 
"110213s.dwg" 
"110347s.dwg" 
"110514s.dwg" 
"110792s.dwg" 
"110793s.dwg" 
"111087s.dwg" 
"111095s.dwg" 
"111101s.dwg" 
"111103s.dwg" 
"111117s.dwg" 
"111124s.dwg" 
"112676s.dwg" 
"112678s.dwg" 
"112765s.dwg" 
"112809s.dwg" 
"112926s.dwg" 
"113228s.dwg" 
"113358s.dwg" 
"114148s.dwg" 
"114179s.dwg" 
"114180s.dwg" 
"114287s.dwg" 
"114536s.dwg" 
"114595s.dwg" 
"114613s.dwg" 
"114629s.dwg" 
"114632s.dwg" 
"114638s.dwg" 
"114641s.dwg" 
"114645s.dwg" 
"114648s.dwg" 
"114651s.dwg" 
"114654s.dwg" 
"114681s.dwg" 
"114687s.dwg" 
"114691s.dwg" 
"114695s.dwg" 
"114818s.dwg" 
"114819s.dwg" 
"114904s.dwg" 
"114996s.dwg" 
"115156s.dwg" 
"115157s.dwg" 
"115189s.dwg" 
"115354s.dwg" 
"115513s.dwg" 
"115959s.dwg" 
"116356s.dwg" 
"116762s.dwg" 
"117032s.dwg" 
"117194s.dwg" 
"117689s.dwg" 
"123771s.dwg" 
"124109s.dwg" 
"124112s.dwg" 
"124115s.dwg" 
"124118s.dwg" 
"124121s.dwg" 
"124124s.dwg" 
"124170s.dwg" 
"124558s.dwg" 
"124561s.dwg" 
"124915s.dwg" 
"125672s.dwg" 
"125976s.dwg" 
"126314s.dwg" 
"126318s.dwg" 
"126392s.dwg" 
"126393s.dwg" 
"126453s.dwg" 
"126675s.dwg" 
"127537s.dwg" 
"129272s.dwg" 
"129490s.dwg" 
"129492s.dwg" 
"129494s.dwg" 
"129496s.dwg" 
"129498s.dwg" 
"129500s.dwg" 
"129502s.dwg" 
"129504s.dwg" 
"129947s.dwg" 
"129982s.dwg" 
"130009s.dwg" 
"130162s.dwg" 
"130171s.dwg" 
"130180s.dwg" 
"130189s.dwg" 
"130191s.dwg" 
"130193s.dwg" 
"130195s.dwg" 
"130197s.dwg" 
"130199s.dwg" 
"130201s.dwg" 
"130225s.dwg" 
"130228s.dwg" 
"130233s.dwg" 
"130282s.dwg" 
"130283s.dwg" 
"130309s.dwg" 
"130313s.dwg" 
"130317s.dwg" 
"130321s.dwg" 
"130365s.dwg" 
"130372s.dwg" 
"130378s.dwg" 
"130390s.dwg" 
"130425s.dwg" 
"130426s.dwg" 
"130431s.dwg" 
"130438s.dwg" 
"130441s.dwg" 
"130445s.dwg" 
"130448s.dwg" 
"130450s.dwg" 
"130476s.dwg" 
"130481s.dwg" 
"130512s.dwg" 
"130583s.dwg" 
"130619s.dwg" 
"130621s.dwg" 
"130681s.dwg" 
"130684s.dwg" 
"131368s.dwg" 
"131788s.dwg" 
"131789s.dwg" 
"131790s.dwg" 
"131791s.dwg" 
"131863s.dwg" 
"131995s.dwg" 
"131998s.dwg" 
"132004s.dwg" 
"132007s.dwg" 
"132010s.dwg" 
"132013s.dwg" 
"132016s.dwg" 
"132019s.dwg" 
"132022s.dwg" 
"132025s.dwg" 
"132123s.dwg" 
"132194s.dwg" 
"132198s.dwg" 
"132200s.dwg" 
"132202s.dwg" 
"132213s.dwg" 
"132219s.dwg" 
"132224s.dwg" 
"132229s.dwg" 
"132234s.dwg" 
"132239s.dwg" 
"132479s.dwg" 
"132488s.dwg" 
"132492s.dwg" 
"132502s.dwg" 
"132505s.dwg" 
"132508s.dwg" 
"132511s.dwg" 
"132515s.dwg" 
"132592s.dwg" 
"132771s.dwg" 
"132775s.dwg" 
"132777s.dwg" 
"132779s.dwg" 
"132781s.dwg" 
"132783s.dwg" 
"132785s.dwg" 
"132787s.dwg" 
"132789s.dwg" 
"132791s.dwg" 
"132793s.dwg" 
"132795s.dwg" 
"132797s.dwg" 
"132808s.dwg" 
"132810s.dwg" 
"132812s.dwg" 
"132814s.dwg" 
"132816s.dwg" 
"132827s.dwg" 
"132829s.dwg" 
"132831s.dwg" 
"132833s.dwg" 
"132882s.dwg" 
"132928s.dwg" 
"133285s.dwg" 
"133286s.dwg" 
"133287s.dwg" 
"133289s.dwg" 
"133418s.dwg" 
"133421s.dwg" 
"133424s.dwg" 
"133427s.dwg" 
"133430s.dwg" 
"133433s.dwg" 
"133436s.dwg" 
"133439s.dwg" 
"133442s.dwg" 
"133445s.dwg" 
"133448s.dwg" 
"133451s.dwg" 
"133454s.dwg" 
"133457s.dwg" 
"133463s.dwg" 
"133492s.dwg" 
"133499s.dwg" 
"133521s.dwg" 
"133522s.dwg" 
"133523s.dwg" 
"133524s.dwg" 
"133525s.dwg" 
"133543s.dwg" 
"133705s.dwg" 
"133903s.dwg" 
"134005s.dwg" 
"134100s.dwg" 
"134104s.dwg" 
"134512s.dwg" 
"134520s.dwg" 
"134537s.dwg" 
"134539s.dwg" 
"134542s.dwg" 
"134552s.dwg" 
"134553s.dwg" 
"134554s.dwg" 
"134556s.dwg" 
"134557s.dwg" 
"134558s.dwg" 
"134561s.dwg" 
"134562s.dwg" 
"134563s.dwg" 
"134564s.dwg" 
"134628s.dwg" 
"135026s.dwg" 
"135027s.dwg" 
"135187s.dwg" 
"135365s.dwg" 
"135367s.dwg" 
"135368s.dwg" 
"135369s.dwg" 
"135374s.dwg" 
"135375s.dwg" 
"135376s.dwg" 
"135972s.dwg" 
"136582s.dwg" 
"137020s.dwg" 
"137025s.dwg" 
"137030s.dwg" 
"137035s.dwg" 
"137509s.dwg" 
"137514s.dwg" 
"137517s.dwg" 
"137595s.dwg" 
"137911s.dwg" 
"138470s.dwg" 
"138473s.dwg" 
"138490s.dwg" 
"138499_inners.dwg" 
"138499_outers.dwg" 
"138506_inners.dwg" 
"138506_outers.dwg" 
"138532s.dwg" 
"138533s.dwg" 
"138534s.dwg" 
"138535s.dwg" 
"138536s.dwg" 
"138537s.dwg" 
"138538s.dwg" 
"138539s.dwg" 
"138603s.dwg" 
"220002s.dwg" 
"220003s.dwg" 
"220004s.dwg" 
"220005s.dwg" 
"220006s.dwg" 
"220008s.dwg" 
"220010s.dwg" 
"220012s.dwg" 
"220014s.dwg" 
"220016s.dwg" 
"220020s.dwg" 
"220525s.dwg" 
"220601s.dwg" 
"220603s.dwg" 
"234250s.dwg" 
"234300s.dwg" 
"234400s.dwg" 
"234500s.dwg" 
"234600s.dwg" 
"Mdsl32s.dwg" 
"Mdsl50s.dwg" 
"Mdsl70s.dwg" 
"tube50s.dwg" 
"tube75s.dwg" 
))
(foreach e list_of_files
(setq ibloque e)
(c:DWGGenerateElementRevitFamily)
)
(setvar "CMDECHO" cmdecho)  
)

(DEFUN C:DWGGenerateElementRevitFamily()
(setq nMater(+ nMater  1)) 
 (print nMater)
 (print ibloque)
 (setq btoInsert (strcat "C:/Temp/Revit_2026/s/" ibloque))
 (setq Master (strcat "Master_"  (itoa nMater)))
 (print Master)
 (print btoInsert)
 (setq iwblock ibloque)
 (setq iwblock2(vl-string-subst "r" "s" iwblock))
 (print iwblock2)
 (setq bltoCreate (strcat "C:/Temp/Revit_2026/s/r/" iwblock2))
 (print bltoCreate)
(command "_.insert" btoInsert "0,0" "1.0" "1.0" "")
(command "._explode" (cdr (assoc -1 (entget (entlast)))))
(c:juan4)
(command "._explode" (cdr (assoc -1 (entget (entlast)))))
   (setq clayer (getvar "clayer")
   expert (getvar "expert")
		)
  (setvar "expert" 1)
  (command "_.layer" "_M" "Z_PERI LoD 3D"  "") 
  (command "_.layer" "_UNL" "*" "")
  (command "_.layer" "_f" "*" "")
  (setq obj3d (getvar "Z_PERI LoD 3D"))
  ;;(command "._layer" "_om" "*" "_on" clayer "")
  (command "._layer" "_off" "Z_PERI LoD 3D" "_on" clayer "")
  (command "._layer" "_f" "Z_PERI LoD 3D" "_on" clayer "")
  (command "._layer" "_on" "0" "")
  (command "._layer" "_t"  "0"  "")
  (command "._layer" "_on" "Z_PERI LoD 3D" "")
  (command "._layer" "_t"  "Z_PERI LoD 3D"  "")
  (command "._layer" "_M" "0"  "") 
  (command "_select" "_all" "")
 ;; (setq SSet (ssget "_P"))
(command "._explode" (cdr (assoc -1 (entget (entlast)))))
  (command "_select" "_all" "")
  (setq SSetEblock (ssget "_P"))
  (command "._block" Master "0,0" SSetEblock "")
 (command "_.wblock" bltoCreate Master)
 (print Master)
 (command "_.layer" "_UNL" "*" "")
 (command "_.layer" "_t" "*" "")
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (Command "_.-PURGE" "_A" "" "_N") ;Limpia todo
 (command "._erase" "_all" "" "")
  (setvar "expert" expert)
) 
(DEFUN C:juan4 ()
 (vlax-for b (vla-get-blocks (vla-get-activedocument (vlax-get-acad-object)))
    (if	(= 0 (vlax-get b 'islayout) (vlax-get b 'isxref))
      (vlax-put b 'explodable -1)
    )
  )
  )
 
(DEFUN C:Sonia ()
  (VL-CMDF "_undo" "_m")
  (VL-CMDF "_EXPERT" "5")
  (setq nhh 0)
  (setq H (ssget))
  (setq nh(sslength h))
  (SETQ NL1 NH)
  (setq l(SSNAME H nhh))
  (setq count 0)
  (VL-CMDF "_select" l "")
  (VL-CMDF "_zoom" "_c" "0,0,0" "0.1")
;;Sonia
   (while (< count 37)
	(VL-CMDF "_copy" "_P" "" "0,0,0"  "0,0,0")
	(VL-CMDF "_move" "_l" "" "0,0,0"  "-3.4,0,380")
	(setq count (1+ count))
  )
  (VL-CMDF "_select" "_p" "")
    (setq count 0)
;;Sonia
  (while (< count 26)
	(VL-CMDF "_copy" "_P" "" "0,0,0"  "0,0,0")
	(VL-CMDF "_move" "_l" "" "0,0,0"  "-3.4,7.3,380")
	(setq H (ssget "_P"))
	 
    (setq nhh 0)
    (setq nh(sslength H))
    (SETQ NL1 NH)
    (setq l(SSNAME H nhh))
    (setq PT(ENTGET L))
    (setq PINS(cdr (assoc 10 PT)))
        ;;Sonia
	(VL-CMDF "_rotate" "_p" "" PINS  "0.9")
	(VL-CMDF "_select" "_p" "")
	(setq count (1+ count))
  )
    (VL-CMDF "_zoom" "_p"  "")
	(print "Ya ta Sonia")
)
(DEFUN C:SoniaP1 ()
  (VL-CMDF "_undo" "_m")
  (VL-CMDF "_EXPERT" "5")
  (setq nhh 0)
  (setq H (ssget))
  (setq nh(sslength h))
  (SETQ NL1 NH)
  (setq l(SSNAME H nhh))
  (setq PT(ENTGET L))
  (setq PPL(cdr (assoc 210 PT)))
  (setq PINS(cdr (assoc 10 PT)))
  (setq AINS(cdr (assoc 50 PT)))
  (setq ANBL(cdr (assoc 2 PT)))
  (Setq AINS(rtd AINS))
  (print pt)
  (print pins)
  (print ains)
  (print anbl)
  (setq count 0)
  (setq ptx -3.4);
  (setq pty 0);
  (setq ptz 380);
  (VL-CMDF "_select" l "")
  (while (< count 37)
	(VL-CMDF "_copy" "_P" "" "0,0,0"  "0,0,0")
	(VL-CMDF "_move" "_l" "" "0,0,0"  "-3.4,0,380")
	(VL-CMDF "_select" "_p" "")
	(setq count (1+ count))
  )
    (setq count 0)
    (while (< count 26)
	(VL-CMDF "_copy" "_P" "" "0,0,0"  "0,0,0")
	(VL-CMDF "_move" "_l" "" "0,0,0"  "-3.4,7.3,380")
	(VL-CMDF "_rotate" "_p" "" "0,0,0"  "0.9")
	(VL-CMDF "_select" "_p" "")
	(setq count (1+ count))
  )
)


(DEFUN C:juan ()
  (VL-CMDF "_undo" "_m")
  (VL-CMDF "_EXPERT" "5")
  (setq nhh 0)
  (setq H (ssget))
  (setq nh(sslength h))
  (SETQ NL1 NH)
  (setq l(SSNAME H nhh))
  (setq PT(ENTGET L))
  (setq PPL(cdr (assoc 210 PT)))
  (setq PINS(cdr (assoc 10 PT)))
  (setq AINS(cdr (assoc 50 PT)))
  (setq ANBL(cdr (assoc 2 PT)))
  (Setq AINS(rtd AINS))
  (print pt)
  (print pins)
  (print ains)
  (print anbl)
)



;;**********************************************************************************
;;** tools
;;**********************************************************************************
(setq CMDECHOOriginal (getvar "CMDECHO"))
(setq OSMODEOriginal (getvar "OSMODE"))

(defun c:refin ()
  (setq nref (getvar "OSMODE"))
)

 

(defun c:e()
  (VL-CMDF "_Extend")
)

(defun c:di()
  (VL-CMDF "_subtract")
)

(defun c:t()
  (VL-CMDF "_trim")
)

(defun c:Mi()
  (VL-CMDF "_MIRROR")
)

(defun c:O()
  (VL-CMDF "_OFFSET")
)

(defun c:refin2	()
  (VL-CMDF "OSMODE" nref)
)

(defun dtr (g)
  (* pi (/ g 180.0))
)

(defun rtd (r)
  (/ (* r 180.0) pi)
)

(defun c:CMDECHOon ()
	(c:CMDECHOoff)
	(setvar "CMDECHO" 0)
)

(defun c:CMDECHOoff ()
	(setvar "CMDECHO" CMDECHOOriginal)
)

(defun c:b ()
	(VL-CMDF "_erase")
)  
(defun c:d ()
	(VL-CMDF "_move")
) 
(defun wait (seconds stop)
	(while (> stop (getvar "DATE"))
	)
)

(defun c:SectionSelectPoints ()
	(setq pt1 (getpoint "\nSelect the first point: "))
	(setq pt2 (getpoint pt1 "\nSelect the second point: "))
	(c:CalculateAngles pt1 pt2)
)

(defun c:CalculateAngles (pt1 pt2)
	(setq pi90   (* 0.5 pi))
	(setq pi270  (* 1.5 pi))
	(setq ang0   (angle pt1 pt2))
	(setq ang0d  (rtd ang0))
	(setq ang90  (+ ang0 pi90))
	(setq ang90d (rtd ang90))
	(setq ang18  (+ ang0 pi))
	(setq ang18d (rtd ang18))
	(setq ang27  (+ ang0 pi270))
	(setq ang27d (rtd ang27))
	(setq dist   (distance pt1 pt2))
)

(defun c:pp	()
  (LOAD "E:/00Maven/25237CAD2/CSD/Contents/Resources/Lisp/set.lsp")
  (setq j22 900)
)

(defun c:d1	()
  (LOAD "C:/Workspaces/SelfDevelopment/Spike_32610_CAD/SET/Contents/Resources/Lisp/set.lsp")
  (prompt "\nset.lsp reloaded ")
)

;;**********************************************************************************
;;** sections
;;**********************************************************************************
(defun c:SET_CreateSection (Xaxis XYplane)
	(c:CMDECHOon)
	(VL-CMDF "_undo" "_m")
	(c:SectionSaveCurrentStatus) 
	(c:SectionSelectObjects) 
	(c:SET_ChangeViewAndUCS Xaxis XYplane "_v")
	(c:CMDECHOoff)
	(prompt "\nThe section has been created. ")
)

(defun c:SET_ChangeViewAndUCS (a b c)
	(VL-CMDF "_vpoint" "_r"  a b)
	(VL-CMDF "_ucs" c)
)

(defun c:SectionSaveCurrentStatus ()
    (VL-CMDF "_expert" "5")
    (VL-CMDF "_view" "_s" "VSECCION")
    (VL-CMDF "_ucs" "_s" "SCPSECCION")
)

(defun c:SectionSelectObjects (/ selection count object)
	(defun Dxf (Id Obj)	(cdr (assoc Id (entget Obj))))	
	(prompt "\nSelect object(s) to keep: ")
	(cond
		(
			(setq selection (ssget))
			(VL-CMDF "_select" "_all" "_r" selection "")
			(setq selection (ssget "_P"))
       
			(repeat (setq count (sslength selection))
				(setq count (1- count) object (ssname selection count))
				(if (/= 4 (logand 4 (Dxf 70 (tblobjname "_.layer" (Dxf 8 object)))))
					(if (Dxf 60 object)
						(entmod (subst '(60 . 1) (assoc 60 (entget object)) (entget object)))
						(entmod (append (entget object) (list '(60 . 1))))
					)
					(prompt "\nThe entity is on a locked layer. Cannot select this entity. ")
				)
			)
		)
	)
)

(defun c:SET_CreateSection2Points ()
	(c:CMDECHOon)
	(VL-CMDF "_undo" "_m")
	(c:SectionSaveCurrentStatus)
	(c:SectionSelectPoints)
	(c:SectionSelectObjects)
	(VL-CMDF "_UCS" "_z" ang18d)
	(VL-CMDF "_UCS" "_X" "90")
	(VL-CMDF "_plan" "")
	(c:CMDECHOoff)
	(prompt "\nThe section has been created ")
)

(defun c:SET_IsolateDetail (/)
	(c:CMDECHOon)
	(VL-CMDF "_undo" "_m")
    (progn
		(c:SectionSaveCurrentStatus)
		(c:SectionSelectObjects)
    )    
	(c:CMDECHOoff)
	(prompt "\nThe detail has been isolated ")
)

(defun c:SET_RecoverOriginalView (/)
	(c:CMDECHOon)
	(VL-CMDF "_undo" "_m")
	(cond
	(
		(setq selection (ssget "_X" '((60 . 1))))
		(repeat (setq count (sslength selection))
			(setq count (1- count) Elem (ssname selection count))
			(if (/= 4 (logand 4 (Dxf 70 (tblobjname "layer" (Dxf 8 Elem)))))
				(entmod (subst '(60 . 0) '(60 . 1) (entget Elem)))
			)
		)
		(VL-CMDF "_view" "_r" "VSECCION")
		(VL-CMDF "_ucs" "_r" "SCPSECCION")
	)
	)
	(c:CMDECHOoff)
	(prompt "\nThe section has been restored ")
)

(defun c:SET_ChangeBlockType (blockType)
	(c:CMDECHOon)
	(VL-CMDF "_undo" "_m")
	(setq ss "nil")
	(setq ent "nil")
	(setq obj "nil")
	(vl-load-com)
	(if (not jmm-replaceall)
		(setq jmm-replaceall "Single")
	)
	(if (and (setq ss (ssget '((0 . "INSERT")))))
		(progn
			(if (eq jmm-replaceall "Global")
				(setq ss (ssget "x" (list '(0 . "INSERT") (assoc 2 (entget (ssname ss 0))))))
			)
			(setq idx -1)
			(while (setq ent (ssname ss (setq idx (1+ idx))))
				(setq listbl (entget ent))
				(setq NomBChange (CDR (ASSOC 2 listbl)))
				(setq obj (vlax-ename->vla-object ent))
				(SET_LoadBlock NomBChange)
				(setq NomBChange(substr NomBChange 1 (- (strlen NomBChange) 1)))
				(setq newname (strcat NomBChange blockType))
				(if (/= newname "nil")
					(tblobjname "BLOCK" newname)
				)
				(if (/= newname "nil")
					(vla-put-name obj newname)
				)
				(if (/= newname "nil")
					(vla-update obj)
				)
			)
		)
    )
	(princ (strcat "\nReplaced " (itoa idx) " blocks "))
)



;;**********************************************************************************
;;**********************************************************************************

(defun c:david ()
  (SET_InsertBlock "100009" 0 0 0 0 0) 
)  

(defun c:sc ()
 (VL-CMDF "_STRETCH")
)  


(defun c:b ()
 (VL-CMDF "_erase")
)  

(defun c:d ()
 (VL-CMDF "_move")
)  
  
(defun c:180 ()
(VL-CMDF "_VPOINT" "R" "0" "0" )
(VL-CMDF "_ucs"  "_V")
)
(defun c:270 ()
(VL-CMDF "_VPOINT" "R" "270" "0" )
(VL-CMDF "_ucs"  "V")
)
(defun c:0 ()
(VL-CMDF "_VPOINT" "R" "270" "90" )
(VL-CMDF "_ucs"  "V")
)
(defun c:p (/)
(VL-CMDF "_VPOINT" "")
)
 
 (defun c:SET_InsertUnitArea (Module ModuleWidht ModuleLength)
 	(setq ModuleLength1 ModuleLength)
	(setq pi90   (* 0.5 pi))
	(setq pi270  (* 1.5 pi))
 	(setq pt1 (getpoint "\nSelect the first point: "))
	(setq pt1i pt1)
	(setq pt2i (getcorner pt1 "\nSelect the second point: "))
 	(setq pt11(polar pt1 0 1000))
	(setq pt22(polar pt2i pi270 1000))
	(setq pt33(polar pt1 pi90 1000))
	(setq pt44(polar pt2i pi 1000))
	(SETQ Pt2 (INTERS pt1 pt11 pt2i pt22 NIL))
	(SETQ Pt3 (INTERS pt1 pt33 pt2i pt44 NIL))
	(c:CalculateAngles pt1 pt2)
	(setq distand(distance pt1 pt2))
	(c:SET_InsertUnitArea2)
 	(c:SET_InsertUnitArea3)
  	)
	(defun c:SET_InsertUnitArea2 ()
	(setq nmodulo(/ distand ModuleWidht))
	(setq nmodulo2(fix nmodulo))
	(SETQ PP11 (CAR pt1))
	(SETQ PP22 (CADR pt1))
	(SETQ PP33 0)
	(SET_Insertblock Module PP11 PP22 PP33 ang0D 3 "false" "true")
	(SETQ NNOD(SSGET "l"))
	(setq NNODl(SSNAME NNOD 0))
	(setq vlaobj(vlax-ename->vla-object NNODl))
	(setq sibloqued (vlax-get-property vlaobj 'isdynamicblock))
	(if (= sibloqued :vlax-true)
	(progn
      		(setq variables (vla-getdynamicblockproperties vlaobj))
       		(setq valores(vlax-variant-value variables))
       		(setq lista(vlax-safearray->list valores))
			(setq total_valores(length lista))
			(setq contador 0)
			(setq valor2 0)
			(while (< contador total_valores)
				(setq valor(vlax-get-property(nth contador lista)"Value"))
				(SETQ valor0(vlax-variant-type valor))
				(setq valor00(vlax-variant-value valor))
	    		(if (= (vlax-get-property(nth contador lista)"PropertyName")"DistHorizontal")
               		(progn 
					(setq valor2 valor)
						(if (= ModuleWidht 250)(progn (vlax-put-property(nth contador lista)"value" 250.00)))
						(if (= ModuleWidht 500)(progn (vlax-put-property(nth contador lista)"value" 500.00)))
						(if (= ModuleWidht 750)(progn (vlax-put-property(nth contador lista)"value" 750.00)))
						(if (= ModuleWidht 1000)(progn (vlax-put-property(nth contador lista)"value" 1000.00)))
						(if (= ModuleWidht 1500)(progn (vlax-put-property(nth contador lista)"value" 1500.00)))
						(if (= ModuleWidht 2000)(progn (vlax-put-property(nth contador lista)"value" 2000.00)))
						(if (= ModuleWidht 2500)(progn (vlax-put-property(nth contador lista)"value" 2500.00)))  
						(if (= ModuleWidht 3000)(progn (vlax-put-property(nth contador lista)"value" 3000.00)))                      		
					)
				);end IF
				(if (= (vlax-get-property(nth contador lista)"PropertyName")"DistVertical")
               		(progn 
						(setq valor2 valor)
						(if (= ModuleLength 250)(progn (vlax-put-property(nth contador lista)"value" 250.00)))
						(if (= ModuleLength 500)(progn (vlax-put-property(nth contador lista)"value" 500.00)))
						(if (= ModuleLength 750)(progn (vlax-put-property(nth contador lista)"value" 750.00)))
						(if (= ModuleLength 1000)(progn (vlax-put-property(nth contador lista)"value" 1000.00)))
						(if (= ModuleLength 1500)(progn (vlax-put-property(nth contador lista)"value" 1500.00)))
						(if (= ModuleLength 2000)(progn (vlax-put-property(nth contador lista)"value" 2000.00)))
						(if (= ModuleLength 2500)(progn (vlax-put-property(nth contador lista)"value" 2500.00)))  
						(if (= ModuleLength 3000)(progn (vlax-put-property(nth contador lista)"value" 3000.00)))                      		
					)
				);end If
				(setq contador(+ contador  1))
			);end while
			(setq cont 1)
			(setq pt1Ic(polar pt1 Ang0 ModuleWidht)) 
			(while (< cont nmodulo2)
	 			(VL-CMDF "_copy" "_L" "" pt1 pt1Ic)
				(setq cont (+ cont 1))
				(setq pt1 pt1Ic) 
				(setq pt1Ic(polar pt1Ic Ang0 ModuleWidht)) 
			)
		)
	)

 )

 	(defun c:SET_InsertUnitArea3 ()
		(setq pt1 pt1i) 
		(setq ModuleLength2 ModuleLength1)
		 (setq Dist31 (distance pt1i pt3))
         (setq Dist31 (- Dist31 ModuleLength1))    
		 (setq Dist32(/ Dist31 ModuleLength1))
		 (setq Dist33(fix Dist32))
		 (setq RestoDist33 (* ModuleLength1 Dist33))
		 (setq RestoDist33 (- Dist31 RestoDist33))

		(while (> Dist33 0)
		(setq pt1(polar pt1i Ang90 ModuleLength2)) 
		(setq Dist33(- Dist33 1))
		(setq ModuleLength2(+ ModuleLength2 ModuleLength1))
    	(c:SET_InsertUnitArea2)
		)
	)
(defun c:SET_InsertUnit (Module ModuleWidht ModuleLength)
    (VL-CMDF "_expert" "0")
	(c:CMDECHOon)
	(c:SectionSelectPoints)
	(setq distand(distance pt1 pt2))
	(setq nmodulo(/ distand ModuleWidht))
	(setq nmodulo2(fix nmodulo))
	(SETQ PP11 (CAR pt1))
    (SETQ PP22 (CADR pt1))
	(SETQ PP33 0)
	(SET_Insertblock Module PP11 PP22 PP33 ang0D 3 "false" "true")
	(SETQ NNOD(SSGET "l"))
	(setq NNODl(SSNAME NNOD 0))
	(setq vlaobj(vlax-ename->vla-object NNODl))
	(setq sibloqued (vlax-get-property vlaobj 'isdynamicblock))
	(if (= sibloqued :vlax-true)
		(progn
      		(setq variables (vla-getdynamicblockproperties vlaobj))
       		(setq valores(vlax-variant-value variables))
       		(setq lista(vlax-safearray->list valores))
			(setq total_valores(length lista))
			(setq contador 0)
			(setq valor2 0)
			(while (< contador total_valores)
				(setq valor(vlax-get-property(nth contador lista)"Value"))
				(SETQ valor0(vlax-variant-type valor))
				(setq valor00(vlax-variant-value valor))
	    		(if (= (vlax-get-property(nth contador lista)"PropertyName")"DistHorizontal")
               		(progn 
    					(setq valor2 valor)
						(if (= ModuleWidht 250)(progn (vlax-put-property(nth contador lista)"value" 250.00)))
						(if (= ModuleWidht 500)(progn (vlax-put-property(nth contador lista)"value" 500.00)))
						(if (= ModuleWidht 750)(progn (vlax-put-property(nth contador lista)"value" 750.00)))
						(if (= ModuleWidht 1000)(progn (vlax-put-property(nth contador lista)"value" 1000.00)))
						(if (= ModuleWidht 1500)(progn (vlax-put-property(nth contador lista)"value" 1500.00)))
						(if (= ModuleWidht 2000)(progn (vlax-put-property(nth contador lista)"value" 2000.00)))
						(if (= ModuleWidht 2500)(progn (vlax-put-property(nth contador lista)"value" 2500.00)))  
						(if (= ModuleWidht 3000)(progn (vlax-put-property(nth contador lista)"value" 3000.00)))                      		
					)
				);end IF
				(if (= (vlax-get-property(nth contador lista)"PropertyName")"DistVertical")
               		(progn 
    					(setq valor2 valor)
						(if (= ModuleLength 250)(progn (vlax-put-property(nth contador lista)"value" 250.00)))
						(if (= ModuleLength 500)(progn (vlax-put-property(nth contador lista)"value" 500.00)))
						(if (= ModuleLength 750)(progn (vlax-put-property(nth contador lista)"value" 750.00)))
						(if (= ModuleLength 1000)(progn (vlax-put-property(nth contador lista)"value" 1000.00)))
						(if (= ModuleLength 1500)(progn (vlax-put-property(nth contador lista)"value" 1500.00)))
						(if (= ModuleLength 2000)(progn (vlax-put-property(nth contador lista)"value" 2000.00)))
						(if (= ModuleLength 2500)(progn (vlax-put-property(nth contador lista)"value" 2500.00)))  
						(if (= ModuleLength 3000)(progn (vlax-put-property(nth contador lista)"value" 3000.00)))                      		
					)
				);end If
				(setq contador(+ contador  1))
			);end while
		)
	)
) 

(defun c:SET_InsertUnitLength (Module ModuleWidht ModuleLength)
    (VL-CMDF "_expert" "0")
	(c:CMDECHOon)
	(c:SectionSelectPoints)
	;;;(print ModuleWidht)
	;;;(print ModuleLength)
	;;;(print Module)
	(setq distand(distance pt1 pt2))
	(setq nmodulo(/ distand ModuleWidht))
	(setq nmodulo2(fix nmodulo))
	(SETQ PP11 (CAR pt1))
    (SETQ PP22 (CADR pt1))
	(SETQ PP33 0)
	(SET_Insertblock Module PP11 PP22 PP33 ang0D 3 "false" "true")
	(SETQ NNOD(SSGET "l"))
	(setq NNODl(SSNAME NNOD 0))
	(setq vlaobj(vlax-ename->vla-object NNODl))
	(setq sibloqued (vlax-get-property vlaobj 'isdynamicblock))
	(if (= sibloqued :vlax-true)
		(progn
      		(setq variables (vla-getdynamicblockproperties vlaobj))
       		(setq valores(vlax-variant-value variables))
       		(setq lista(vlax-safearray->list valores))
			(setq total_valores(length lista))
			(setq contador 0)
			(setq valor2 0)
			(while (< contador total_valores)
				(setq valor(vlax-get-property(nth contador lista)"Value"))
				(SETQ valor0(vlax-variant-type valor))
				(setq valor00(vlax-variant-value valor))
	    		(if (= (vlax-get-property(nth contador lista)"PropertyName")"DistHorizontal")
               		(progn 
    					(setq valor2 valor)
						(if (= ModuleWidht 250)(progn (vlax-put-property(nth contador lista)"value" 250.00)))
						(if (= ModuleWidht 500)(progn (vlax-put-property(nth contador lista)"value" 500.00)))
						(if (= ModuleWidht 750)(progn (vlax-put-property(nth contador lista)"value" 750.00)))
						(if (= ModuleWidht 1000)(progn (vlax-put-property(nth contador lista)"value" 1000.00)))
						(if (= ModuleWidht 1500)(progn (vlax-put-property(nth contador lista)"value" 1500.00)))
						(if (= ModuleWidht 2000)(progn (vlax-put-property(nth contador lista)"value" 2000.00)))
						(if (= ModuleWidht 2500)(progn (vlax-put-property(nth contador lista)"value" 2500.00)))  
						(if (= ModuleWidht 3000)(progn (vlax-put-property(nth contador lista)"value" 3000.00)))                      		
					)
				);end IF
				(if (= (vlax-get-property(nth contador lista)"PropertyName")"DistVertical")
               		(progn 
    					(setq valor2 valor)
						(if (= ModuleLength 250)(progn (vlax-put-property(nth contador lista)"value" 250.00)))
						(if (= ModuleLength 500)(progn (vlax-put-property(nth contador lista)"value" 500.00)))
						(if (= ModuleLength 750)(progn (vlax-put-property(nth contador lista)"value" 750.00)))
						(if (= ModuleLength 1000)(progn (vlax-put-property(nth contador lista)"value" 1000.00)))
						(if (= ModuleLength 1500)(progn (vlax-put-property(nth contador lista)"value" 1500.00)))
						(if (= ModuleLength 2000)(progn (vlax-put-property(nth contador lista)"value" 2000.00)))
						(if (= ModuleLength 2500)(progn (vlax-put-property(nth contador lista)"value" 2500.00)))  
						(if (= ModuleLength 3000)(progn (vlax-put-property(nth contador lista)"value" 3000.00)))                      		
					)
				);end If
				(setq contador(+ contador  1))
			);end while
			(setq cont 1)
			(setq pt1Ic(polar pt1 Ang0 ModuleWidht)) 
			(while (< cont nmodulo2)
	 			(VL-CMDF "_copy" "_L" "" pt1 pt1Ic)
				(setq cont (+ cont 1))
				(setq pt1 pt1Ic) 
				(setq pt1Ic(polar pt1Ic Ang0 ModuleWidht)) 
			)
		)
	)
) 
(DEFUN c:SCir () 
	(setq ss (ssget '((0 . "CIRCLE"))))
	(SETQ numb (SSLENGTH ss))
	(setq nl (ssname ss 0))
	(setq listbl (entget nl))
)
(defun c:SET_ChangeBay (newname)
;;;;;(print newname)
  (setq ccodigo "nil")
  (setq altura "nil")
  (setq ss "nil")
  (setq ent "nil")
  (setq obj "nil")
  (vl-load-com)
  (setvar "cmdecho" 0)
  (if (not jmm-replaceall)
    (setq jmm-replaceall "Single")
  )
  (if (and (setq ss (ssget '((0 . "INSERT")))))
    (progn
      (if (eq jmm-replaceall "Global")
	(setq ss (ssget "x" (list '(0 . "INSERT") (assoc 2 (entget (ssname ss 0))))))
      )
      (setq idx -1)
      (while (setq ent (ssname ss (setq idx (1+ idx))))
	  (setq obj (vlax-ename->vla-object ent))
	  (setq vlaobj (vlax-ename->vla-object ent))
      (setq sibloqued (vlax-get-property vlaobj 'isdynamicblock))
      (if (= sibloqued :vlax-true)
       (progn
		  (setq variables (vla-getdynamicblockproperties vlaobj))
		  (setq valores (vlax-variant-value variables))
		  (setq lista (vlax-safearray->list valores))
		  (setq total_valores (length lista))
		  (setq contador 0)
		  (setq valor2 0)
		      (while (< contador total_valores)
				(setq valor	(vlax-get-property (nth contador lista) "Value"))
				(SETQ valor0 (vlax-variant-type valor))
				(setq valor00 (vlax-variant-value valor))
					(if (= (vlax-get-property (nth contador lista) "PropertyName")"DistHorizontal")
						(progn(setq DistHor valor00))
					)
					(if (= (vlax-get-property (nth contador lista) "PropertyName")"DistVertical")
						(progn(setq DistVer valor00))
					)

				(setq contador2 contador)
				(setq contador (+ contador 1))
				)
	  )
	 )
 
	(if (/= newname "nil")
	  (tblobjname "BLOCK" newname)
	)
	(if (/= newname "nil")
	  (vla-put-name obj newname)
	)
	(if (/= newname "nil")
	  (vla-update obj)
	)
	 (if (/= DistHor "nil")
			   (progn
			    (setq avlaobj(vlax-ename->vla-object ent))
			     (setq sibloqued (vlax-get-property vlaobj 'isdynamicblock))
				  (if (= sibloqued :vlax-true)
				      (progn
				      	 (setq avariables (vla-getdynamicblockproperties avlaobj))
				       	 (setq avalores(vlax-variant-value avariables))
				       	 (setq alista(vlax-safearray->list avalores))
				         (setq atotal_valores(length alista))
				         (setq acontador 0)
					     (setq avalor2 0)
				          (while (< acontador atotal_valores)
					    	 (setq avalor(vlax-get-property(nth acontador alista)"Value"))
				             	 (SETQ avalor0(vlax-variant-type avalor))
					    	 (setq avalor00(vlax-variant-value avalor))
					    	 (if (= (vlax-get-property(nth acontador alista)"PropertyName")"DistHorizontal")
				                  	(progn
										(vlax-put-property(nth acontador alista)"value" DistHor)			    
									)
				             )
							(if (= (vlax-get-property(nth acontador alista)"PropertyName")"DistVertical")
				                  	(progn
							            (vlax-put-property(nth acontador alista)"value" DistVer)			    
		  					        )  
							 )
							(setq acontador(+ acontador  1)) 
					     )
				       	   
					)
				)   	    
		          )			  
	         )


      )
    )
  )
  (VL-CMDF ".undo" "end")
  (princ (strcat "\nReplaced " (itoa idx) " blocks......"))
  (princ)
)

(DEFUN c:PERIREC_2012 () 
(setq Seconds 1)
	(setq dtexc "")
	(setq nom "")
	(setq dtex "")
	(setq listbl "")
	(SETQ CONT 0)
	(setq nl "")
	;;(setq ss (ssget '((0 . "INSERT"))))
	(setq ss (ssget ))
   (VL-CMDF"_select" ss "") 
   (SETQ numb (SSLENGTH ss))
   (SETQ nj0 (SSGET "_P" ))
	(while (> numb 0)
	  (setq nl (ssname nj0 CONT))
  	  (setq listbl (entget nl))
      (SETQ nom (CDR (ASSOC 2 listbl)))
	  (VL-CMDF"_select" ss "") 
	  (SETQ nj (SSGET "_P" (LIST (CONS 0 "INSERT") (CONS 2 nom))))
      (SETQ num (SSLENGTH nj))
	  (setq x(itoa num))
	  (setq tt (vl-string-search nom dtexc))
	(if (= tt nil)
		(progn
		    (setq dtex(strcat dtex nom "....." x " "))
			(setq dtexc (strcat dtex dtexc))
			;;  (setq http "https://localhost:44300/SDCadCentrio/CadCount?Dato=")
			 ;; (setq getMavenCount (strcat http  dtex))
			 ;; (dcl_Html_Navigate PERI_Present_trabajo_HtmlLoadTemp getMavenCount)
		   (setq dtex "")
		 )
	  )
	  (setq numb (- numb 1))
	  (setq CONT (+ CONT 1))
   )
   ;; ;;;;;(print dtexc)
   (setq LinScaffold "10022149")
   (setq LinJobsite "4")
   (setq myError "null")
   (setq err "0")
   (setq errb "null")
   ;;(dcl_Form_Show PERI_Present_FormCount)
;;   (setq getMaven "https://localhost:44300/SDCadCentrio/InsertCount")
;;   (dcl_Html_Navigate PERI_Present_FormCount_HTMLCount getMaven)
;;(dcl_Form_Close PERI_Present_trabajo)
)
 (defun c:Up3dIndustrialManual#OnClicked (/)
 (dcl_Form_Show PERI_Present_WizardVertical)
 (setq getMavenLoadVertical "https://localhost:44300/SDCadCentrio/Mo3dUp") 
 (dcl_Html_Navigate PERI_Present_WizardVertical_HTNLWV getMavenLoadVertical)
)

 		 
(defun c:SET_AssembleUnit (BaseStandar BaseElements SpindleLocking FirstStandard RestOfStandard DecksAtAllLevels UseUHPlus DecksLevels Elevation NumberOfPlatforms)
(setq NumberOfPlatforms(atof NumberOfPlatforms))
(IF (= FirstStandard "101306")(progn(Setq ElvFirstStandard 1000)))
(IF (= FirstStandard "100009")(progn(Setq ElvFirstStandard 2000)))
(IF (= FirstStandard "100012")(progn(Setq ElvFirstStandard 3000)))
(IF (= FirstStandard "100013")(progn(Setq ElvFirstStandard 4000)))

(setq VElevationd(atof Elevation)) 
(setq VElevation(- VElevationd ElvFirstStandard))
;;;;;(print VElevation)
 
(setq NSetVCont40Ext 0)
(setq NSetVCont30Ext 0)
(setq NSetVCont20Ext 0)
(setq NSetVCont15Ext 0)
(setq NSetVCont10Ext 0)
(setq NSetVCont05Ext 0)
(setq NSetVCont40 0)
(setq NSetVCont30 0)
(setq NSetVCont20 0)
(setq NSetVCont15 0)
(setq NSetVCont10 0)
(setq NSetVCont05 0)
(setq NSetVCont40S1 0)
(setq NSetVCont30S1 0)
(setq NSetVCont20S1 0)
(setq NSetVCont15S1 0)
(setq NSetVCont10S1 0)
(setq NSetVCont05S1 0)

(setq NSetVCont40La 0)
(setq NSetVCont30La 0)
(setq NSetVCont20La 0)
(setq NSetVCont15La 0)
(setq NSetVCont10La 0)
(setq NSetVCont05La 0)
(setq EsScaPlat "0")
 (setq ModuleAccessLadder 0)
;;;;(print "1")
;;Programa 4,00
	(if (= RestOfStandard "100013")
		(progn
		   (setq NSetVCont40Ext(/ VElevation 4000))
		   (setq NSetVCont40Ext(fix NSetVCont40Ext))
		   (setq dist2 (fix(- VElevation (* 4000 NSetVCont40Ext))))
		   (if (= dist2 3000)(progn(setq NSetVCont30Ext 1)))
		   (if (= dist2 2000)(progn(setq NSetVCont20Ext 1)))
		   (if (= dist2 1500)(progn(setq NSetVCont15Ext 1)))
		   (if (= dist2 1000)(progn(setq NSetVCont10Ext 1)))
		   (if (= dist2 500)(progn(setq NSetVCont05Ext 1)))
		
		   (setq alturanumerd(+ VElevation 1000))
		   (setq NSetVCont40(/ alturanumerd 4000))
		   (setq NSetVCont40(fix NSetVCont40))
		   (setq dist2 (fix(- alturanumerd (* 4000 NSetVCont40))))
		   (if (= dist2 3000)(progn (setq NSetVCont30 1)))
		   (if (= dist2 2000)(progn (setq NSetVCont20 1)))
		   (if (= dist2 1500)(progn (setq NSetVCont15 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05 1)))

		   (setq alturanumerLa(- VElevationd 1500))
		   (setq NSetVCont40La(/ alturanumerLa 4000))
		   (setq NSetVCont40La(fix NSetVCont40La))
		   (setq dist2 (fix(- alturanumerLa (* 4000 NSetVCont40La))))
		   (if (= dist2 3000)(progn (setq NSetVCont30La 1)))
		   (if (= dist2 2000)(progn (setq NSetVCont20La 1)))
		   (if (= dist2 1500)(progn (setq NSetVCont15La 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10La 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05La 1)))
		  
		   (setq alturanumerdS1(+ VElevation 4000))
		   (setq NSetVCont40S1(/ alturanumerdS1 4000))
		   (setq NSetVCont40S1(fix NSetVCont40S1))
		   (setq dist2 (fix(- alturanumerdS1 (* 4000 NSetVCont40S1))))
		   (if (= dist2 3000)(progn (setq NSetVCont30S1 1)))
		   (if (= dist2 2000)(progn (setq NSetVCont20S1 1)))
		   (if (= dist2 1500)(progn (setq NSetVCont15S1 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10S1 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05S1 1)))
	  )
	)
	;;;;(print "3")
;;Programa 3,00
	(if (= RestOfStandard "100012")
		(progn
		   (setq NSetVCont30Ext(/ VElevation 3000))
		   (setq NSetVCont30Ext(fix NSetVCont30Ext))
		   (setq dist2 (fix(- VElevation (* 3000 NSetVCont30Ext))))
		   (if (= dist2 2000)(progn(setq NSetVCont20Ext 1)))
		   (if (= dist2 1500)(progn(setq NSetVCont15Ext 1)))
		   (if (= dist2 1000)(progn(setq NSetVCont10Ext 1)))
		   (if (= dist2 500)(progn(setq NSetVCont05Ext 1)))
		   
		   (setq alturanumerd(+ VElevation 1000))
		   (setq NSetVCont30(/ alturanumerd 3000))
		   (setq NSetVCont30(fix NSetVCont30))
		   (setq dist2 (fix(- alturanumerd (* 3000 NSetVCont30))))
		   (if (= dist2 2000)(progn (setq NSetVCont20 1)))
		   (if (= dist2 1500)(progn (setq NSetVCont15 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05 1)))

		   (setq alturanumerLa(- VElevationd 1500))
		   (setq NSetVCont30La(/ alturanumerLa 3000))
		   (setq NSetVCont30La(fix NSetVCont30La))
		   (setq dist2 (fix(- alturanumerLa (* 3000 NSetVCont30La))))
		   (if (= dist2 2000)(progn (setq NSetVCont20La 1)))
		   (if (= dist2 1500)(progn (setq NSetVCont15La 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10La 1)))
		   (if (= dist2 500) (progn (setq NSetVCont05La 1)))

		   (setq alturanumerdS1(+ VElevation 3000))
		   (setq NSetVCont30S1(/ alturanumerdS1 3000))
		   (setq NSetVCont30S1(fix NSetVCont30S1))
		   (setq dist2 (fix(- alturanumerdS1 (* 3000 NSetVCont30S1))))
		   (if (= dist2 2000)(progn (setq NSetVCont20S1 1)))
		   (if (= dist2 1500)(progn (setq NSetVCont15S1 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10S1 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05S1 1)))
	  )
	)
	;;;;(print "4")
;;Programa 2,00	
	(if (= RestOfStandard "100009")
		(progn
		   (setq NSetVCont20Ext(/ VElevation 2000))
		   (setq NSetVCont20Ext(fix NSetVCont20Ext))
		   (setq dist2 (fix(- VElevation (* 2000 NSetVCont20Ext))))
		   (if (= dist2 1500)(progn(setq NSetVCont15Ext 1)))
		   (if (= dist2 1000)(progn(setq NSetVCont10Ext 1)))
		   (if (= dist2 500)(progn(setq NSetVCont05Ext 1)))
		   (setq alturanumerd(+ VElevation 1000))
		   (setq NSetVCont20(/ alturanumerd 2000))
		   (setq NSetVCont20(fix NSetVCont20))
		   (setq dist2 (fix(- alturanumerd (* 2000 NSetVCont20))))
		   (if (= dist2 1500)(progn (setq NSetVCont15 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05 1)))
		   ;;;;(print "22")
		 
		   (setq alturanumerLa(- VElevationd 1500))
		   ;;;;(print "23")
		   (setq NSetVCont20La(/ alturanumerLa 2000))
		   ;;;;(print "24")
		   (setq NSetVCont20La(fix NSetVCont20La))
		   (setq dist2 (fix(- alturanumerLa (* 2000 NSetVCont20La))))
		   (if (= dist2 1500)(progn (setq NSetVCont15La 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10La 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05La 1)))
		   ;;;;(print "26")
		   (setq alturanumerdS1(+ VElevation 2000))
		   (setq NSetVCont20S1(/ alturanumerdS1 2000))
		   (setq NSetVCont20S1(fix NSetVCont20S1))
		   (setq dist2 (fix(- alturanumerdS1 (* 2000 NSetVCont20S1))))
		   (if (= dist2 1500)(progn (setq NSetVCont15S1 1)))
		   (if (= dist2 1000)(progn (setq NSetVCont10S1 1)))
		   (if (= dist2 500) (progn (setq  NSetVCont05S1 1)))
	 )
	)
 ;;;;(print "5")
(setq DistMensuTrav 0)
(setq CDSCode "0")
(setq EsScale "0")
(setq ModPSDS1 "0")
(setq ModBracketPlatform "0")
(setq ExitModAccessDecks "0")
(setq ExitModuleStaircase100125 "0")

(setq CDSCode 0)
(setq DistMensuTrav 0)
 
(setq Seconds 0.05)
(setvar "cmdecho" 0)
;;(setq VElevation Elevation)
;;;;;(print "sS")
(if (= BaseElements "116762")(progn(setq ElvBase 145)))
(if (= BaseElements "100244")(progn(setq ElvBase 5)))
(if (= BaseElements "100411")(progn(setq ElvBase 135)))
(if (= BaseElements "100242")(progn(setq ElvBase 235)))
(if (= BaseElements "019780")(progn(setq ElvBase 496)))
(if (= BaseElements "100159")(progn(setq ElvBase 110)))

(if (= BaseElements "116762")(progn(setq HElvBaseD 145)))
(if (= BaseElements "100244")(progn(setq HElvBaseD 5)))
(if (= BaseElements "100411")(progn(setq HElvBaseD 135)))
(if (= BaseElements "100242")(progn(setq HElvBaseD 235)))
(if (= BaseElements "019780")(progn(setq HElvBaseD 496)))
(if (= BaseElements "100159")(progn(setq HElvBaseD 110)))
	
	(if (= BaseStandar "100014")(progn(setq HElvBaseD (+ HElvBaseD 130))))
	(if (= BaseStandar "117194")(progn(setq HElvBaseD (+ HElvBaseD 380))))
	(if (= BaseStandar "0")(progn(setq HElvBaseD (+ HElvBaseD 380))))
	 
 (setq NCont 1)
 (setq result "000000000")
 (setq ElmentNumber 0)
 (VL-CMDF "_expert" "0")
 (VL-CMDF "_view" "_s" "ud")
  (setq NameBlockLV "")
  (setq ccodigo "nil")
  (setq altura "nil")
  (setq ss "nil")
  (setq ent "nil")
  (setq obj "nil")
;;  (vl-load-com)
  
		  (if (not jmm-replaceall)
			(setq jmm-replaceall "Single")
		  )
		  
  (if (and (setq ss (ssget '((0 . "INSERT")))))
	    (progn
		(if (eq jmm-replaceall "Global")
				   (setq ss (ssget "x" (list '(0 . "INSERT") (assoc 2 (entget (ssname ss 0))))))
		          )
			      (setq idx -1)
				
				  (VL-CMDF "_ucs"  "_w")
				(while (setq ent (ssname ss (setq idx (1+ idx))));;;;while0 
				 (setq lLV (entget ent))
			     (SETQ nom (CDR (ASSOC 2 lLV)))
				 (setq ElmentNumber (+ ElmentNumber 1))
				 (setq obj (vlax-ename->vla-object ent))
				  (setq nl (ssname ss idx))
 				  (setq verticalheightTypeL VElevation)
				  (setq verticalheightTypeS  (- VElevation 2000))
					(setq l (entget nl))
				    (setq pt1 (cdr (assoc 10 l)))
					(setq pt1VG (cdr (assoc 10 l)))
				    (setq NameObject (cdr (assoc 2 l)))
					(setq pi2 (/ pi 2))
					(setq ang0 (cdr (assoc 50 l)))
					(setq ang0D (rtd ang0))
					;;45
					(setq ang225D (+ ang0D 225))
					(setq ang225 (dtr ang225D))
					(setq ang45D (+ ang0D 45))
					(setq ang45 (dtr ang45D))
					(setq ang135D (+ ang0D 135))
					(setq ang135 (dtr ang135D))
					(setq ang315D (+ ang0D 315))
					(setq ang315 (dtr ang315D))					
					(setq ang90d (+ ang0D 90))
					(setq ang90 (dtr ang90d))
					(setq ang27d (+ ang0D 270))
				    (setq ang27 (dtr ang27d))
					(setq ang18(+ ang0  pi))
					(setq ang18D(+ ang0D  180))
					(setq vlaobj (vlax-ename->vla-object ent))
					(setq sibloqued (vlax-get-property vlaobj 'isdynamicblock))
					(if (= sibloqued :vlax-true)
				    		(progn
						      (setq variables (vla-getdynamicblockproperties vlaobj))
						      (setq valores (vlax-variant-value variables))
						      (setq lista (vlax-safearray->list valores))
						      (setq total_valores (length lista))
						      (setq contador 0)
						      (setq valor2 0)
							     (setq pasowhile1 0) 
								 (while (< contador total_valores)
									(setq pasowhile1 (+ pasowhile1 1))
									(setq valor (vlax-get-property (nth contador lista) "Value"))
										(SETQ valor0 (vlax-variant-type valor))
										(setq valor00 (vlax-variant-value valor))
										(if(=(vlax-get-property (nth contador lista) "PropertyName") "DistVertical")
										   (progn
											;; ;;;;;(print "2")
											 (setq DistVerticald (fix valor00))
										     
											  (if(> DistVerticald 745) (setq DistVertical 750))
											  (if(> DistVerticald 995) (setq DistVertical 1000))
											  (if(> DistVerticald 1495)(setq DistVertical 1500))
											  (if(> DistVerticald 1995)(setq DistVertical 2000))
											  (if(> DistVerticald 2495)(setq DistVertical 2500))
											  (if(> DistVerticald 2995)(setq DistVertical 3000))
											  (if(> DistVerticald 3005)(setq DistVertical 0))
										 )
				                        )
										(if(=(vlax-get-property (nth contador lista) "PropertyName") "DistHorizontal")
										   (progn
											   (setq DistHorizontald (fix valor00))
										       (if(> DistHorizontald 745) (setq DistHorizontal 750))
											   (if(> DistHorizontald 995) (setq DistHorizontal 1000))
											   (if(> DistHorizontald 1495)(setq DistHorizontal 1500))
											   (if(> DistHorizontald 1995)(setq DistHorizontal 2000))
											   (if(> DistHorizontald 2495)(setq DistHorizontal 2500))
											   (if(> DistHorizontald 2995)(setq DistHorizontal 3000))
											   (if(> DistHorizontald 3005)(setq DistHorizontal 0))
										  )
				                        )
										(if(=(vlax-get-property (nth contador lista) "PropertyName") "DistMensuTrav")(progn(setq DistMensuTrav valor00)))
										(if(=(vlax-get-property (nth contador lista) "PropertyName") "CDSCode")(progn(setq CDSCode valor00)))
							(setq contador (+ 1 contador))
							)
							)
						)  
				  
				(if (= CDSCode "ModuleAccessLadder")(progn(setq ModuleAccessLadder "1")))		
				(if (= CDSCode "ModStairTowerAlternate")(progn(setq EsScale "1")))
				(if (= CDSCode "ModStairTowerparallel")(progn(setq EsScale "2")))
				
				;;Cambiar  esta chapuza
				(if (= CDSCode "ModStairTowerAlternate")(progn(setq EsScaPlat "1")))
				(if (= CDSCode "ModStairTowerparallel")(progn(setq EsScaPlat "2")))
				(if (= CDSCode "ModPSDS1")(progn(setq ModPSDS1 "1")))
				(if (= CDSCode "ModBracketPlatform")(progn(setq ModBracketPlatform "1")))
				(if (= CDSCode "ModAccessDecks")(progn(setq ExitModAccessDecks "1")))
				(if (= CDSCode "ModuleStaircase100125")(progn(setq ExitModuleStaircase100125 "1")))
			   
			    

			   (c:DefineBlock)
				(if (= ExitModuleStaircase100125 "1")
					(progn
				        (c:ManagerVerticalStaircase100125)
						(c:ManagerHorizontalStaircase100125)
					)
					(progn
					 (if (= ModuleAccessLadder "1")
						 (progn
					      (c:ManagerVerticalModuleAccessLadder)
						  (c:ManagerHorizontal)
						  )
						  (progn
							(c:ManagerVertical)
							(c:ManagerHorizontal)
						 )
				     )
					 )
				)
					(if (= ModPSDS1 "1")(progn(c:ModPSDS1)))
					(if (= ModBracketPlatform "1")(progn(c:ModBracketPlatform)))
(setq CDSCode "0")
(setq EsScale "0")
 
(setq ModPSDS1 "0")
(setq ModBracketPlatform "0")
(setq ExitModAccessDecks "0")
(setq ExitModuleStaircase100125 "0")
(setq DistMensuTrav 0)
(setq ModuleAccessLadder "0")
(setq EsScaPlat "0")

);;Endwhile0
   	 )
   )
 (setvar "cmdecho" 1)
)
 (defun C:ModBracketPlatform ()
	(setq HElvBase HElvBased)
	(setq NpLTD 1) 
	(setq TypeV 0)
	(setq verticalheightTypeL VElevation)
	(SETQ pt1hu(polar pt1VG ang90 DistVertical ))

	(if (= DistHorizontal 1000) (progn(setq ElToeboardTLS1 "110073")))
	(if (= DistHorizontal 1500) (progn(setq ElToeboardTLS1 "110160")))
	(if (= DistHorizontal 2000) (progn(setq ElToeboardTLS1 "110176")))
	(if (= DistHorizontal 2500) (progn(setq ElToeboardTLS1 "110208")))
    (if (= DistHorizontal 3000) (progn(setq ElToeboardTLS1 "110211")))
 
	(if (= DistHorizontal 1000)(progn(setq ElHorizontalTLS1 "114632")))
	(if (= DistHorizontal 1500)(progn(setq ElHorizontalTLS1 "114641")))
	(if (= DistHorizontal 2000)(progn(setq ElHorizontalTLS1 "114645")))
	(if (= DistHorizontal 2500)(progn(setq ElHorizontalTLS1 "114648")))
	(if (= DistHorizontal 3000)(progn(setq ElHorizontalTLS1 "114651")))

	(if (= DistMensuTrav 750)(progn(setq ElConsole "112678")))
	(if (= DistMensuTrav 500)(progn(setq ElConsole "112676")))
   
	(if (= DistMensuTrav 500)(progn(setq ElHorizontalT2S1 "114595")))
	(if (= DistMensuTrav 750)(progn(setq ElHorizontalT2S1 "114629")))



    (if (= DistMensuTrav 750)(progn(setq ElToeboardTS1 "110514")))
	(if (= DistMensuTrav 500)(progn(setq ElToeboardTS1 "110213")))

    (if (= DistHorizontal 750)   (progn(setq ElPlats1 "124121")))
    (if (= DistHorizontal 1000)  (progn(setq ElPlats1 "124118")))
    (if (= DistHorizontal 1500)  (progn(setq ElPlats1 "124112")))
    (if (= DistHorizontal 2000)  (progn(setq ElPlats1 "124109")))
    (if (= DistHorizontal 2500)  (progn(setq ElPlats1 "123771")))
    (if (= DistHorizontal 3000)  (progn(setq ElPlats1 "124915")))

	(setq NumberOfPlatformsv1 NumberOfPlatforms)
    (setq NpLT NpLTD)
 	  (while (> NumberOfPlatformsv1 0)
			(setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(if (= TYpeH "Y")
				   (progn
						  (setq pt1huds1 pt1hu)
						  (SETQ HElvBase1 HElvBase)
						  (SETQ PP11 (CAR pt1hu))
						  (SETQ PP22 (CADR pt1hu))
						  (SETQ PP33 HElvBase1)
						  (SETQ PH1 (LIST PP11 PP22 PP33))
						  (SET_Insertblock ElConsole PP11 PP22 PP33 ang90D 3 "false")
						 (SETQ PH1S(POLAR PH1 ANG0 DistHorizontal))
						    (SETQ PPH11S (CAR PH1S))
						    (SETQ PPH22S (CADR PH1S))
							(SET_Insertblock ElConsole  PPH11S PPH22S PP33 ang90D 3 "false")
						;;Insertar Rodapie
						 (SET_Insertblock ElToeboardTS1 PP11 PP22 PP33 ang90D 3 "false") 
					        (SETQ PH1S(POLAR PH1S ANG90 DistMensuTrav))
							(SETQ PPH11S (CAR PH1S))
						    (SETQ PPH22S (CADR PH1S))
							(SET_Insertblock ElToeboardTS1 PPH11S PPH22S PP33 ang27D 3 "false")
					 
					 
					;;Insertar Plataformas
						   (setq DisPlat DistMensuTrav)
						 (SETQ PPL1(POLAR PH1 ANG90 125))
						   (SETQ PPLL1 (CAR PPL1))
						   (SETQ PPLL2 (CADR PPL1))
						   (SETQ PP33 HElvBase1)
						  (while (> DisPlat 0)
							  (SET_Insertblock ElPlats1 PPLL1 PPLL2 PP33 ang90D 3 "false") 
							  (SETQ PPL1(POLAR PPL1 ANG90 250))
					   	      (SETQ PPLL1 (CAR PPL1))
						      (SETQ PPLL2 (CADR PPL1))
							  (SETQ DisPlat(- DisPlat 250))
						  )
								   (setq HElvBase2(+ HElvBase1 500))
								   (SETQ PP11 (CAR PH1))
								   (SETQ PP22 (CADR PH1))
								   (SETQ PP33 HElvBase2)
								   (SETQ PTh05 (LIST PP11 PP22 PP33))
								   (SET_Insertblock ElHorizontalT2S1 PP11 PP22 PP33 ang90D 3 "false") 
								   (SETQ PTh05S(POLAR PTh05 ANG0 DistHorizontal))
								   (SETQ PP115S (CAR PTh05S))
								   (SETQ PP225S (CADR PTh05S))
								   (SET_Insertblock ElHorizontalT2S1 PP115S PP225S PP33 ang90D 3 "false") 
								   (setq HElvBase2(+ HElvBase2 500))
								   (SETQ PP33 HElvBase2)
								   (SETQ PTh05 (LIST PP11 PP22 PP33))
								   (SETQ PP115 (CAR PTh05))
								   (SETQ PP225 (CADR PTh05))
								   (SET_Insertblock ElHorizontalT2S1 PP115 PP225 PP33 ang90D 3 "false") 
								   (SETQ PTh05S(POLAR PTh05 ANG0 DistHorizontal))
								   (SETQ PP115S (CAR PTh05S))
								   (SETQ PP225S (CADR PTh05S))
								   (SET_Insertblock ElHorizontalT2S1 PP115S PP225S PP33 ang90D 3 "false") 
							   (SETQ PTSR1(polar PH1 ang90 DistMensuTrav))
							   (setq HElvBase4(+ HElvBase1 100))
								   (SETQ PP11 (CAR PTSR1))
								   (SETQ PP22 (CADR PTSR1))
								   (SETQ PP33 HElvBase4)
								   (SETQ PThV (LIST PP11 PP22 PP33))
							  (SET_Insertblock "101306" PP11 PP22 PP33 ang90D 3 "false") 
							   (SETQ PThV(POLAR PThV ANG0 DistHorizontal))
							   (SETQ PP11V (CAR PThV))
							   (SETQ PP22V (CADR PThV))
							  (SET_Insertblock "101306" PP11V PP22V PP33 ang90D 3 "false")
							   (setq HElvBase3 HElvBase)
						       (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
						       (SETQ PP33 HElvBase3)
						       (SETQ PTSR1 (LIST PP11 PP22 PP33))
						       (SET_Insertblock ElToeboardTLS1 PP11 PP22 PP33 ang0D 3 "false") 
							   (setq HElvBase3(+ HElvBase3 500))
							   (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
							   (SETQ PP33 HElvBase3)
							   (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SET_Insertblock ElHorizontalTLS1 PP11 PP22 PP33 ang0D 3 "false") 
							   (setq HElvBase3(+ HElvBase3 500))
							   (SETQ PP33 HElvBase3)
							   (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SET_Insertblock ElHorizontalTLS1 PP11 PP22 PP33 ang0D 3 "false") 
					)
				)
	 (SETQ HElvBase(+ HElvBase 2000))
	)
	
 )


 (defun c:ModPSDS1 () 
	(setq HElvBase HElvBased)
	(setq NpLTD 1) 
	(setq TypeV 0)
	(setq verticalheightTypeL VElevation)
	(SETQ pt1hu(polar pt1VG ang90 DistVertical ))
	(if (= DistHorizontal 1000) (progn(setq ElToeboardTLS1 "110073")))
	(if (= DistHorizontal 1500) (progn(setq ElToeboardTLS1 "110160")))
	(if (= DistHorizontal 2000) (progn(setq ElToeboardTLS1 "110176")))
	(if (= DistHorizontal 2500) (progn(setq ElToeboardTLS1 "110208")))
    (if (= DistHorizontal 3000) (progn(setq ElToeboardTLS1 "110211")))
 
	(if (= DistHorizontal 1000)(progn(setq ElHorizontalTLS1 "114632")))
	(if (= DistHorizontal 1500)(progn(setq ElHorizontalTLS1 "114641")))
	(if (= DistHorizontal 2000)(progn(setq ElHorizontalTLS1 "114645")))
	(if (= DistHorizontal 2500)(progn(setq ElHorizontalTLS1 "114648")))
	(if (= DistHorizontal 3000)(progn(setq ElHorizontalTLS1 "114651")))

 	(if (= DistMensuTrav 1000) (progn(setq ElToeboardTS1 "110073")))
	(if (= DistMensuTrav 1500) (progn(setq ElToeboardTS1 "110160")))
	(if (= DistMensuTrav 2000) (progn(setq ElToeboardTS1 "110176")))
	(if (= DistMensuTrav 2500) (progn(setq ElToeboardTS1 "110208")))
    (if (= DistMensuTrav 3000) (progn(setq ElToeboardTS1 "110211")))
	;;AQUI


 	(if (= DistMensuTrav 1000) (progn(setq ElHorizontalLS1 "114632")))
	(if (= DistMensuTrav 1500) (progn(setq ElHorizontalLS1 "114641")))
	(if (= DistMensuTrav 2000) (progn(setq ElHorizontalLS1 "114645")))
	(if (= DistMensuTrav 2500) (progn(setq ElHorizontalLS1 "114648")))
    (if (= DistMensuTrav 3000) (progn(setq ElHorizontalLS1 "114651")))

	(if (= DistMensuTrav 1000)(progn(setq ElHorizontalT2S1 "114632")))
	(if (= DistMensuTrav 1500)(progn(setq ElHorizontalT2S1 "114641")(if (= UseUHPlus "True")(progn(setq ElHorizontalT2S1 "114681")))))
	(if (= DistMensuTrav 2000)(progn(setq ElHorizontalT2S1 "114645")(if (= UseUHPlus "True")(progn(setq ElHorizontalT2S1 "114687")))))
	(if (= DistMensuTrav 2500)(progn(setq ElHorizontalT2S1 "114648")(if (= UseUHPlus "True")(progn(setq ElHorizontalT2S1 "114691")))))
	(if (= DistMensuTrav 3000)(progn(setq ElHorizontalT2S1 "114651")(if (= UseUHPlus "True")(progn(setq ElHorizontalT2S1 "114695")))))
  
    (if (= DistHorizontal 750)   (progn(setq ElPlats1 "124121")))
    (if (= DistHorizontal 1000)  (progn(setq ElPlats1 "124118")))
    (if (= DistHorizontal 1500)  (progn(setq ElPlats1 "124112")))
    (if (= DistHorizontal 2000)  (progn(setq ElPlats1 "124109")))
    (if (= DistHorizontal 2500)  (progn(setq ElPlats1 "123771")))
    (if (= DistHorizontal 3000)  (progn(setq ElPlats1 "124915")))

	(if (= DistMensuTrav 1500)(progn(setq ElDiagonalS1 "100572")))
	(if (= DistMensuTrav 2000)(progn(setq ElDiagonalS1 "100573")))
	(if (= DistMensuTrav 2500)(progn(setq ElDiagonalS1 "100574")))
	(if (= DistMensuTrav 3000)(progn(setq ElDiagonalS1 "100575")))
 
	(if (< DistMensuTrav 1499)
			(progn
			(c:ModPSDS1MENOR)
			)
			(progn
			(c:ModPSDS1MAYOR)
			)
	)
 )
 (defun c:ModPSDS1MAYOR()
 (setq NumberOfPlatformsv1 NumberOfPlatforms)
 (setq NpLT NpLTD)
 	  (while (> NumberOfPlatformsv1 0)
			(setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(if (= TYpeH "Y")
				   (progn
				  ;;;; ;;;;;(print "23")
				   ;;Primer Modulo laterial izq.
						  (setq pt1huds1 pt1hu)
						  (SETQ HElvBase1 HElvBase)
						  (SETQ PP11 (CAR pt1hu))
						  (SETQ PP22 (CADR pt1hu))
						  (SETQ PP33 HElvBase1)
						  (SETQ PH1 (LIST PP11 PP22 PP33))
						  ;;ElPlats1
						  ;;Insert Modulo General y Plataformas
						  (SET_Insertblock ElHorizontalT2S1 PP11 PP22 PP33 ang90D 3 "false")
						 ;; ;;;;;(print "24")
						(SETQ PH1S(POLAR PH1 ANG0 DistHorizontal))
						    (SETQ PPH11S (CAR PH1S))
						    (SETQ PPH22S (CADR PH1S))
							(SET_Insertblock ElHorizontalT2S1  PPH11S PPH22S PP33 ang90D 3 "false")
						;;;;;(print "25")
						;;Insertar Rodapie
						 (SET_Insertblock ElToeboardTS1 PP11 PP22 PP33 ang90D 3 "false") 
							;;;;;(print "26")
							(SET_Insertblock ElHorizontalT2S1   PPH11S PPH22S PP33 ang90D 3 "false") 
					       ;; ;;;;;(print "27")
							(SETQ PH1S(POLAR PH1S ANG90 DistMensuTrav))
							(SETQ PPH11S (CAR PH1S))
						    (SETQ PPH22S (CADR PH1S))
							(SET_Insertblock ElToeboardTS1 PPH11S PPH22S PP33 ang27D 3 "false")
							;;;;;(print "28")
					;;Insertar Plataformas
						   (setq DisPlat DistMensuTrav)
						 (SETQ PPL1(POLAR PH1 ANG90 125))
						   (SETQ PPLL1 (CAR PPL1))
						   (SETQ PPLL2 (CADR PPL1))
						   (SETQ PP33 HElvBase1)
						  (while (> DisPlat 0)
							  (SET_Insertblock ElPlats1 PPLL1 PPLL2 PP33 ang90D 3 "false") 
							;;;;;(print "29")
							(SETQ PPL1(POLAR PPL1 ANG90 250))
					   	      (SETQ PPLL1 (CAR PPL1))
						      (SETQ PPLL2 (CADR PPL1))
							  (SETQ DisPlat(- DisPlat 250))
						  )
								  ;;Insert ParteS LateralES, horizontales 
								   (setq HElvBase2(+ HElvBase1 500))
								   (SETQ PP11 (CAR PH1))
								   (SETQ PP22 (CADR PH1))
								   (SETQ PP33 HElvBase2)
								   (SETQ PTh05 (LIST PP11 PP22 PP33))
								   (SET_Insertblock ElHorizontalLS1 PP11 PP22 PP33 ang90D 3 "false") 
								  ;; ;;;;;(print "30")
								   (SETQ PTh05S(POLAR PTh05 ANG0 DistHorizontal))
								   (SETQ PP115S (CAR PTh05S))
								   (SETQ PP225S (CADR PTh05S))
								   (SET_Insertblock ElHorizontalLS1 PP115S PP225S PP33 ang90D 3 "false") 
								 ;; ;;;;;(print "31")
								  ;;mIRAR EL 420
								   (setq HElvBase2(+ HElvBase2 500))
								   (SETQ PP33 HElvBase2)
								   (SETQ PTh05 (LIST PP11 PP22 PP33))
								   (SETQ PP115 (CAR PTh05))
								   (SETQ PP225 (CADR PTh05))
								   (SET_Insertblock ElHorizontalLS1 PP115 PP225 PP33 ang90D 3 "false") 
								  ;; ;;;;;(print "32")
								   (SETQ PTh05S(POLAR PTh05 ANG0 DistHorizontal))
								   (SETQ PP115S (CAR PTh05S))
								   (SETQ PP225S (CADR PTh05S))
								   (SET_Insertblock ElHorizontalLS1 PP115S PP225S PP33 ang90D 3 "false") 
							      ;; ;;;;;(print "33")
							 
							   (SETQ PTSR1(polar PH1 ang90 DistMensuTrav))
							       (SETQ PP11 (CAR PTSR1))
								   (SETQ PP22 (CADR PTSR1))
								   (SETQ PP33 (- HElvBase2 1000))
							   (SET_Insertblock ElDiagonalS1 PP11 PP22 PP33 ang27D 3 "false") 
							 ;; ;;;;;(print "34")
							 (setq HElvBase4(- HElvBase1 400))
						      (SETQ PThV (LIST PP11 PP22 PP33))
							  (SETQ PP33 HElvBase4)
							  (SET_Insertblock "102860" PP11 PP22 PP33 ang90D 3 "false") 
							 ;; ;;;;;(print "35")
							  (SETQ PThV(POLAR PThV ANG0 DistHorizontal))
							  (SETQ PP11V (CAR PThV))
							  (SETQ PP22V (CADR PThV))
							  (SETQ PP33 (- HElvBase2 1000))
							  (setq PThVS(polar PThV ANG90 1000)) 
							  (SET_Insertblock ElDiagonalS1 PP11V PP22V PP33 ang27D 3 "false") 
							 ;; ;;;;;(print "35")
							  (VL-CMDF "_MIRROR" "_L" "" PThV PThVS "_YES")
							   (SETQ PP33 HElvBase4)
							  (SET_Insertblock "102860" PP11V PP22V PP33 ang90D 3 "false")
						      ;; ;;;;;(print "36")
							   (setq HElvBase3 HElvBase)
						       (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
						       (SETQ PP33 HElvBase3)
						       (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
						       (SET_Insertblock ElToeboardTLS1 PP11 PP22 PP33 ang0D 3 "false") 
							  ;; ;;;;;(print "37")
							   (setq HElvBase3(+ HElvBase3 500))
							   (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
							   (SETQ PP33 HElvBase3)
							   (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SET_Insertblock ElHorizontalTLS1 PP11 PP22 PP33 ang0D 3 "false") 
							  ;; ;;;;;(print "38")
							   (setq HElvBase3(+ HElvBase3 500))
							   (SETQ PP33 HElvBase3)
							   (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SET_Insertblock ElHorizontalTLS1 PP11 PP22 PP33 ang0D 3 "false") 
								;;;;;(print "39")
					)
				)
	 (SETQ HElvBase(+ HElvBase 2000))
	)

 )
 (defun c:ModPSDS1MENOR()
 (setq NumberOfPlatformsv1 NumberOfPlatforms)
 (setq NpLT NpLTD)
 	  (while (> NumberOfPlatformsv1 0)
			(setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(if (= TYpeH "Y")
				   (progn
				   ;;Primer Modulo laterial izq.
						  (setq pt1huds1 pt1hu)
						  (SETQ HElvBase1 HElvBase)
						  (SETQ PP11 (CAR pt1hu))
						  (SETQ PP22 (CADR pt1hu))
						  (SETQ PP33 HElvBase1)
						  (SETQ PH1 (LIST PP11 PP22 PP33))
						  ;;ElPlats1
						  ;;Insert Modulo General y Plataformas
						  (SET_Insertblock ElHorizontalT2S1 PP11 PP22 PP33 ang90D 3 "false")
						 (SETQ PH1S(POLAR PH1 ANG0 DistHorizontal))
						    (SETQ PPH11S (CAR PH1S))
						    (SETQ PPH22S (CADR PH1S))
							(SET_Insertblock ElHorizontalT2S1  PPH11S PPH22S PP33 ang90D 3 "false")
						;;Insertar Rodapie
						 (SET_Insertblock ElToeboardTS1 PP11 PP22 PP33 ang90D 3 "false") 
							(SET_Insertblock ElHorizontalT2S1   PPH11S PPH22S PP33 ang90D 3 "false") 
					        (SETQ PH1S(POLAR PH1S ANG90 DistMensuTrav))
							(SETQ PPH11S (CAR PH1S))
						    (SETQ PPH22S (CADR PH1S))
							(SET_Insertblock ElToeboardTS1 PPH11S PPH22S PP33 ang27D 3 "false")
					 ;;Insertar diagonal
						  (SETQ HElvBase1D(- HElvBase 2000))
						  (SETQ PP33 HElvBase1D)
						  (SETQ PHD1 (LIST PP11 PP22 PP33))
						  (SETQ PHD1S(POLAR PHD1 ANG90 DistHorizontal))
						  (SETQ PPH11D (CAR PHD1))
						  (SETQ PPH22D (CADR PHD1))
						  (SET_Insertblock "112926" PPH11D PPH22D PP33 ang90D 3 "false") 
						  (VL-CMDF "_MIRROR" "_L" "" PHD1 PHD1S "_YES")
						 ;; ;;;;;(print "d3")
						  (SETQ PHD1(POLAR PHD1 ANG0 DistHorizontal))
					      (SETQ PPH11DDS (CAR PHD1))
						  (SETQ PPH22DDS (CADR PHD1))
						  (SET_Insertblock "112926" PPH11DDS PPH22DDS PP33 ang90D 3 "false") 
					;;Insertar Plataformas
						   (setq DisPlat DistMensuTrav)
						 (SETQ PPL1(POLAR PH1 ANG90 125))
						   (SETQ PPLL1 (CAR PPL1))
						   (SETQ PPLL2 (CADR PPL1))
						   (SETQ PP33 HElvBase1)
						  (while (> DisPlat 0)
							  (SET_Insertblock ElPlats1 PPLL1 PPLL2 PP33 ang90D 3 "false") 
							  (SETQ PPL1(POLAR PPL1 ANG90 250))
					   	      (SETQ PPLL1 (CAR PPL1))
						      (SETQ PPLL2 (CADR PPL1))
							  (SETQ DisPlat(- DisPlat 250))
						  )
								  ;;Insert ParteS LateralES, horizontales 
								   (setq HElvBase2(+ HElvBase1 500))
								   (SETQ PP11 (CAR PH1))
								   (SETQ PP22 (CADR PH1))
								   (SETQ PP33 HElvBase2)
								   (SETQ PTh05 (LIST PP11 PP22 PP33))
								   (SET_Insertblock ElHorizontalT2S1 PP11 PP22 PP33 ang90D 3 "false") 
								  
								   (SETQ PTh05S(POLAR PTh05 ANG0 DistHorizontal))
								   (SETQ PP115S (CAR PTh05S))
								   (SETQ PP225S (CADR PTh05S))
								   (SET_Insertblock ElHorizontalT2S1 PP115S PP225S PP33 ang90D 3 "false") 
								  ;;mIRAR EL 420
								   (setq HElvBase2(+ HElvBase2 500))
								   (SETQ PP33 HElvBase2)
								   (SETQ PTh05 (LIST PP11 PP22 PP33))
								   (SETQ PP115 (CAR PTh05))
								   (SETQ PP225 (CADR PTh05))
								   (SET_Insertblock ElHorizontalT2S1 PP115 PP225 PP33 ang90D 3 "false") 
								   (SETQ PTh05S(POLAR PTh05 ANG0 DistHorizontal))
								   (SETQ PP115S (CAR PTh05S))
								   (SETQ PP225S (CADR PTh05S))
								   (SET_Insertblock ElHorizontalT2S1 PP115S PP225S PP33 ang90D 3 "false") 
							   ;;Insertar parte frontal Vertical 1,50 y horizontales frontales
							   (SETQ PTSR1(polar PH1 ang90 DistMensuTrav))
							   (setq HElvBase4(- HElvBase1 400))
								   (SETQ PP11 (CAR PTSR1))
								   (SETQ PP22 (CADR PTSR1))
								   (SETQ PP33 HElvBase4)
								   (SETQ PThV (LIST PP11 PP22 PP33))
							  (SET_Insertblock "102860" PP11 PP22 PP33 ang90D 3 "false") 
							   (SETQ PThV(POLAR PThV ANG0 DistHorizontal))
							   (SETQ PP11V (CAR PThV))
							   (SETQ PP22V (CADR PThV))
							  (SET_Insertblock "102860" PP11V PP22V PP33 ang90D 3 "false")
						       (setq HElvBase3 HElvBase)
						       (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
						       (SETQ PP33 HElvBase3)
						       (SETQ PTSR1 (LIST PP11 PP22 PP33))
						       (SET_Insertblock ElToeboardTLS1 PP11 PP22 PP33 ang0D 3 "false") 
							   (setq HElvBase3(+ HElvBase3 500))
							   (SETQ PP11 (CAR PTSR1))
						       (SETQ PP22 (CADR PTSR1))
							   (SETQ PP33 HElvBase3)
							   (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SET_Insertblock ElHorizontalTLS1 PP11 PP22 PP33 ang0D 3 "false") 
							   (setq HElvBase3(+ HElvBase3 500))
							   (SETQ PP33 HElvBase3)
							   (SETQ PTSR1 (LIST PP11 PP22 PP33))
							   (SET_Insertblock ElHorizontalTLS1 PP11 PP22 PP33 ang0D 3 "false") 
					)
				)
	 (SETQ HElvBase(+ HElvBase 2000))
	)

 )
 
(defun c:ManagerVertical() 
(if (and (= exit180 1) 
         (= exit225 1) 
         (= exit270 1)
		 )
	(setq TypeV 0)
	(setq TypeV 1)
	)
	(SETQ TypeS1 "0")
 
(c:InsertVertical)

;Vert pt90 
  (if (and (= exit180 1) 
           (= exit135 1) 
           (= exit90 1)
	   )
			(setq TypeV 0)
			(setq TypeV 1)
	)
  (setq pt1(polar pt1VG ang90 DistVertical))
  (if (> DistMensuTrav 1499)
			(progn
			(SETQ TypeS1 "1")
			)
  )
  (if (/= exit90 "1")(c:InsertVertical))
  (if (and (= exit90 1) 
           (= exit45 1) 
           (= exit0 1)
	   )
			(setq TypeV 0)
			(setq TypeV 1)
	)
  (setq pt1(polar pt1VG ang0  DistHorizontal))
   (setq pt1(polar pt1 ang90  DistVertical))
 
 (if (/= exit90 1) 
	(progn
		(if (= exit0 1) 
          (progn(if (= TypeDifP0 "1")(progn(c:InsertVertical))))
		  (progn(c:InsertVertical))
	  )
	)
	)
	

  ;Vert pt180 
  (if (and (= exit0 1) 
           (= exit315 1) 
           (= exit270 1)
	   )
			(setq TypeV 0)
			(setq TypeV 1)
	)
  (setq pt1(polar pt1VG ang0  DistHorizontal))
 (SETQ TypeS1 "0")

 (if (/= exit0 1)(c:InsertVertical))

 ) 





(defun c:InsertVertical ()
    (setq PTL PT1)
	(setq ELvBaseL ELvBase)
	(setq NSetVCont40T NSetVCont40)
	(setq NSetVCont30T NSetVCont30)
	(setq NSetVCont20T NSetVCont20)
	(setq NSetVCont15T NSetVCont15)
	(setq NSetVCont10T NSetVCont10)
	(setq NSetVCont05T NSetVCont05)
	(setq NSetVCont40ExtT NSetVCont40Ext)
	(setq NSetVCont30ExtT NSetVCont30Ext)
	(setq NSetVCont20ExtT NSetVCont20Ext)
	(setq NSetVCont15ExtT NSetVCont15Ext)
	(setq NSetVCont10ExtT NSetVCont10Ext)
	(setq NSetVCont05ExtT NSetVCont05Ext)
    ;;Aqui empiezo con el vertical con las bases 
	 (SETQ PP11 (CAR PTL))
	 (SETQ PP22 (CADR PTL))
	 (SETQ PP33 0)
	   (SET_Insertblock BaseElements PP11 PP22 PP33 ang0D 3 "false") 
	 
	 (SETQ PP33 ELvBaseL)
		   (if (= SpindleLocking "100863")
		   (progn
			 (SET_Insertblock SpindleLocking PP11 PP22 PP33 ang0D 3 "false")))
		;;Aqui continuo con UVB
	
		(if (= BaseStandar "100014")
		(progn
		  (SET_Insertblock BaseStandar PP11 PP22 PP33 ang0D 3 "false")   
		  (setq ELvBaseL(+ ELvBaseL 240))
		))
		(if (= BaseStandar "117194")
		   (progn
			(SET_Insertblock BaseStandar PP11 PP22 PP33 ang0D 3 "false")   
			(setq ELvBaseL(+ ELvBaseL 490))
		))
		
		  (SETQ PP33 ELvBaseL)
		  (SET_Insertblock FirstStandard PP11 PP22 PP33 ang0D 3 "false") 
		
		 (setq ELvBaseL (+ ELvBaseL ElvFirstStandard))
		;;Vertical Exterior
		(if (= TypeV 1)
			(progn
				(if (= TypeS1 "1")
					(progn
						(SETQ NSetVCont40 NSetVCont40S1)
						(SETQ NSetVCont30 NSetVCont30S1)
						(SETQ NSetVCont20 NSetVCont20S1)
						(SETQ NSetVCont15 NSetVCont15S1)
						(SETQ NSetVCont10 NSetVCont10S1)
						(SETQ NSetVCont05 NSetVCont05S1)
						)
				)
				(while (> NSetVCont40 0)
					    (SETQ PP33 ELvBaseL)
						(SET_Insertblock "100013" PP11 PP22 PP33 ang0D 3 "false") 
						(setq NSetVCont40(- NSetVCont40 1))
						(setq ELvBaseL (+ ELvBaseL 4000))
				 )
				 
				(while (> NSetVCont30 0)
				(SETQ PP11 (CAR PTL))
					              (SETQ PP33 ELvBaseL)
						
						(SET_Insertblock "100012" PP11 PP22 PP33 ang0D 3 "false") 
						(setq NSetVCont30(- NSetVCont30 1))
						(setq ELvBaseL (+ ELvBaseL 3000))
				 )
        
				(while (> NSetVCont20 0)
				;;;;;(print NSetVCont20)
				(SETQ PP33 ELvBaseL)
								  (SET_Insertblock "100009" PP11 PP22 PP33 ang0D 3 "false") 
							  	  (setq NSetVCont20(- NSetVCont20 1))
								  (setq ELvBaseL (+ ELvBaseL 2000))
				)
			 
				(if (= NSetVCont15  1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "102860" PP11 PP22 PP33 ang0D 3 "false") 
		  						  (setq ELvBaseL (+ ELvBaseL 1500))
				 ))
				 	

				(if (= NSetVCont10  1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "101306" PP11 PP22 PP33 ang0D 3 "false") 						
		  						  (setq ELvBaseL (+ ELvBaseL 1000))
				 ))
				 
				(if (= NSetVCont05  1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "102859" PP11 PP22 PP33 ang0D 3 "false") 						
								  (setq ELvBaseL (+ ELvBaseL 500))
				 ))
			)
				(progn
						 (while (> NSetVCont40Ext  0)
					    (SETQ PP33 ELvBaseL)
						(SET_Insertblock "100013" PP11 PP22 PP33 ang0D 3 "false") 
						(setq NSetVCont40Ext (- NSetVCont40Ext  1))
						(setq ELvBaseL (+ ELvBaseL 4000))
				 )
				(while (> NSetVCont30Ext  0)
				(SETQ PP11 (CAR PTL))
					              (SETQ PP33 ELvBaseL)
						(SET_Insertblock "100012" PP11 PP22 PP33 ang0D 3 "false") 
						(setq NSetVCont30Ext (- NSetVCont30Ext  1))
						(setq ELvBaseL (+ ELvBaseL 3000))
				 )
				(while (> NSetVCont20Ext 0)
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "100009" PP11 PP22 PP33 ang0D 3 "false") 
							  	  (setq NSetVCont20Ext (- NSetVCont20Ext  1))
								  (setq ELvBaseL (+ ELvBaseL 2000))
				)
				 
				(if (= NSetVCont15Ext 1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "102860" PP11 PP22 PP33 ang0D 3 "false") 
		  						  (setq ELvBaseL (+ ELvBaseL 1500))
				 ))
				(if (= NSetVCont10Ext 1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "101306" PP11 PP22 PP33 ang0D 3 "false") 						
		  						  (setq ELvBaseL (+ ELvBaseL 1000))
				 ))
				(if (= NSetVCont05Ext 1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "102859" PP11 PP22 PP33 ang0D 3 "false") 						
								  (setq ELvBaseL (+ ELvBaseL 500))
				 ))
			)
 )
 	(setq NSetVCont40 NSetVCont40T)
	(setq NSetVCont30 NSetVCont30T)
	(setq NSetVCont20 NSetVCont20T)
	(setq NSetVCont15 NSetVCont15T)
	(setq NSetVCont10 NSetVCont10T)
	(setq NSetVCont05 NSetVCont05T)
	(setq NSetVCont40Ext NSetVCont40ExtT)
	(setq NSetVCont30Ext NSetVCont30ExtT)
	(setq NSetVCont20Ext NSetVCont20ExtT)
	(setq NSetVCont15Ext NSetVCont15ExtT)
	(setq NSetVCont10Ext NSetVCont10ExtT)
	(setq NSetVCont05Ext NSetVCont05ExtT)
 )
 
(defun c:ManagerHorizontal()
	(setq NpLTD 1) 
	(setq TypeV 0) 
	(if (= DistHorizontal 250) (progn(setq ElHorizontal "104780")))
	(if (= DistHorizontal 500) (progn(setq ElHorizontal "104779")))

	(if (= DistHorizontal 720) (progn(setq ElHorizontal "114124")))
	(if (= DistHorizontal 750) (progn(setq ElHorizontal "114629")))
	(if (= DistHorizontal 1000)(progn(setq ElHorizontal "114632")))
	(if (= DistHorizontal 1250)(progn(setq ElHorizontal "114638")))
	(if (= DistHorizontal 1500)(progn(setq ElHorizontal "114641")))
	(if (= DistHorizontal 2000)(progn(setq ElHorizontal "114645")))
	(if (= DistHorizontal 2500)(progn(setq ElHorizontal "114648")))
	(if (= DistHorizontal 3000)(progn(setq ElHorizontal "114651")))

	(if (= DistHorizontal 720) (progn(setq ElDiagonal "114124")))
	(if (= DistHorizontal 750) (progn(setq ElDiagonal "124170")))
	(if (= DistHorizontal 1000)(progn(setq ElDiagonal "112926")))
	(if (= DistHorizontal 1250)(progn(setq ElDiagonal "114638")))
	(if (= DistHorizontal 1500)(progn(setq ElDiagonal "100572")))
	(if (= DistHorizontal 2000)(progn(setq ElDiagonal "100573")))
	(if (= DistHorizontal 2500)(progn(setq ElDiagonal "100574")))
	(if (= DistHorizontal 3000)(progn(setq ElDiagonal "100575")))
  
    (if (= DistHorizontal 750)  (progn(setq ElToeboard "110213")))
	(if (= DistHorizontal 1000) (progn(setq ElToeboard "110073")))
	(if (= DistHorizontal 1500) (progn(setq ElToeboard "110160")))
	(if (= DistHorizontal 2000) (progn(setq ElToeboard "110176")))
	(if (= DistHorizontal 2500) (progn(setq ElToeboard "110208")))
    (if (= DistHorizontal 3000) (progn(setq ElToeboard "110211")))

    (if (= DistHorizontal 750)   (progn(setq ElPlat "124121")))
    (if (= DistHorizontal 1000)  (progn(setq ElPlat "124118")))
    (if (= DistHorizontal 1500)  (progn(setq ElPlat "124112")))
    (if (= DistHorizontal 2000)  (progn(setq ElPlat "124109")))
    (if (= DistHorizontal 2500)  (progn(setq ElPlat "123771")))
    (if (= DistHorizontal 3000)  (progn(setq ElPlat "124915")))
	(setq verticalheightTypeL VElevation)
	(SETQ pt1hu pt1VG)
	(if (and (= exit180 1) 
			 (= exit225 1) 
			 (= exit270 1)
	    ) 
		(setq Type1V 0)
		(setq Type1V 1)
	)
	(if (and (= exit0 1) 
             (= exit315 1) 
             (= exit270 1)
	    )
			(setq Type2V 0)
			(setq Type2V 1)
	)
	(setq TypeV (+ Type2V Type1V))
		(if (= exit270 1)
		(setq TypeV 1)
		(setq TypeV 2)
	    )
    ;;Primer Horizontal
	 
	(c:InsertHorizontal)
	
	(setq verticalheightTypeL VElevation)
	(setq pt1hu(polar pt1VG ang90  DistVertical))
	(setq pt1hu(polar pt1hu ang0  DistHorizontal))
			(if (and (= exit180 1) 
					   (= exit135 1) 
					   (= exit90 1)
			         )
				(setq Type1V 0)
				(setq Type1V 1)
			)

		    (if (and (= exit90 1) 
			       (= exit45 1) 
		          (= exit0 1)
			   )
				(setq Type2V 0)
				(setq Type2V 1)
			)
	 
	(setq TypeV (+ Type2V Type1V))
	(if (= exit90 1)
		(setq TypeV 1)
		(setq TypeV 2)
	    )
    ;;SEGUNDO VERTICAL
	 
	(c:InsertHorizontal2)
	(setq TypeV 0)
	(if (= DistVertical 720) (progn(setq ElHorizontalTR "114124")))
	(if (= DistVertical 750) (progn(setq ElHorizontalTR "114629")))
	(if (= DistVertical 1000)(progn(setq ElHorizontalTR "114632")))
	(if (= DistVertical 1250)(progn(setq ElHorizontalTR "114638")))
	(if (= DistVertical 1500)(progn(setq ElHorizontalTR "114641")(if (= UseUHPlus "True")(progn(setq ElHorizontalTR "114681")))))
	(if (= DistVertical 2000)(progn(setq ElHorizontalTR "114645")(if (= UseUHPlus "True")(progn(setq ElHorizontalTR "114687")))))
	(if (= DistVertical 2500)(progn(setq ElHorizontalTR "114648")(if (= UseUHPlus "True")(progn(setq ElHorizontalTR "114691")))))
	(if (= DistVertical 3000)(progn(setq ElHorizontalTR "114651")(if (= UseUHPlus "True")(progn(setq ElHorizontalTR "114695")))))
		
	(if (= DisDifP0 250) (progn(setq ElHorizontalTRP0 "104780")))
	(if (= DisDifP0 500) (progn(setq ElHorizontalTRP0 "104779")))
	(if (= DisDifP0 720) (progn(setq ElHorizontalTRP0 "114124")))
	(if (= DisDifP0 750) (progn(setq ElHorizontalTRP0 "114629")))
	(if (= DisDifP0 1000)(progn(setq ElHorizontalTRP0 "114632")))
	(if (= DisDifP0 1250)(progn(setq ElHorizontalTRP0 "114638")))
	(if (= DisDifP0 1500)(progn(setq ElHorizontalTRP0 "114641")(if (= UseUHPlus "True")(progn(setq ElHorizontalTRP0 "114681")))))
	(if (= DisDifP0 2000)(progn(setq ElHorizontalTRP0 "114645")(if (= UseUHPlus "True")(progn(setq ElHorizontalTRP0 "114687")))))
	(if (= DisDifP0 2500)(progn(setq ElHorizontalTRP0 "114648")(if (= UseUHPlus "True")(progn(setq ElHorizontalTRP0 "114691")))))
	(if (= DisDifP0 3000)(progn(setq ElHorizontalTRP0 "114651")(if (= UseUHPlus "True")(progn(setq ElHorizontalTRP0 "114695")))))
	
	(if (= DistVertical 720) (progn(setq ElHorizontalT "114124")))
	(if (= DistVertical 750) (progn(setq ElHorizontalT "114629")))
	(if (= DistVertical 1000)(progn(setq ElHorizontalT "114632")))
	(if (= DistVertical 1250)(progn(setq ElHorizontalT "114638")))
	(if (= DistVertical 1500)(progn(setq ElHorizontalT "114641")))
	(if (= DistVertical 2000)(progn(setq ElHorizontalT "114645")))
	(if (= DistVertical 2500)(progn(setq ElHorizontalT "114648")))
	(if (= DistVertical 3000)(progn(setq ElHorizontalT "114651")))
 
 	(if (= DisDifP0 250) (progn(setq ElHorizontalTP0 "104780")))
	(if (= DisDifP0 500) (progn(setq ElHorizontalTP0 "104779")))
    (if (= DisDifP0 720) (progn(setq ElHorizontalTP0 "114124")))
	(if (= DisDifP0 750) (progn(setq ElHorizontalTP0 "114629")))
	(if (= DisDifP0 1000)(progn(setq ElHorizontalTP0 "114632")))
	(if (= DisDifP0 1250)(progn(setq ElHorizontalTP0 "114638")))
	(if (= DisDifP0 1500)(progn(setq ElHorizontalTP0 "114641")))
	(if (= DisDifP0 2000)(progn(setq ElHorizontalTP0 "114645")))
	(if (= DisDifP0 2500)(progn(setq ElHorizontalTP0 "114648")))
	(if (= DisDifP0 3000)(progn(setq ElHorizontalTP0 "114651")))

	(if (= DisDifP02 250) (progn(setq ElHorizontalTDF "104780")))
	(if (= DisDifP02 500) (progn(setq ElHorizontalTDF "104779")))
	(if (= DisDifP02 720) (progn(setq ElHorizontalTDF "114124")))
	(if (= DisDifP02 750) (progn(setq ElHorizontalTDF "114629")))
	(if (= DisDifP02 1000)(progn(setq ElHorizontalTDF "114632")))
	(if (= DisDifP02 1250)(progn(setq ElHorizontalTDF "114638")))
	(if (= DisDifP02 1500)(progn(setq ElHorizontalTDF "114641")))
	(if (= DisDifP02 2000)(progn(setq ElHorizontalTDF "114645")))
	(if (= DisDifP02 2500)(progn(setq ElHorizontalTDF "114648")))
	(if (= DisDifP02 3000)(progn(setq ElHorizontalTDF "114651")))

	 
	(if (= DistVertical 720) (progn(setq ElDiagonalT "114124")))
	(if (= DistVertical 750) (progn(setq ElDiagonalT "124170")))
	(if (= DistVertical 1000)(progn(setq ElDiagonalT "112926")))
	(if (= DistVertical 1250)(progn(setq ElDiagonalT "114638")))
	(if (= DistVertical 1500)(progn(setq ElDiagonalT "100572")))
	(if (= DistVertical 2000)(progn(setq ElDiagonalT "100573")))
	(if (= DistVertical 2500)(progn(setq ElDiagonalT "100574")))
	(if (= DistVertical 3000)(progn(setq ElDiagonalT "100575")))
 
    (if (= DistVertical 750)  (progn(setq ElToeboardT "110213")))
	(if (= DistVertical 1000) (progn(setq ElToeboardT "110073")))
	(if (= DistVertical 1500) (progn(setq ElToeboardT "110160")))
	(if (= DistVertical 2000) (progn(setq ElToeboardT "110176")))
	(if (= DistVertical 2500) (progn(setq ElToeboardT "110208")))
    (if (= DistVertical 3000) (progn(setq ElToeboardT "110211")))
  ;;cambiar 250 y 500
  	(if (= DisDifP0 250) (progn(setq ElHorizontalTDF "104780")))
	(if (= DisDifP0 500) (progn(setq ElHorizontalTDF "104779")))
    (if (= DisDifP0 750)  (progn(setq ElToeboardTP0 "110213")))
	(if (= DisDifP0 1000) (progn(setq ElToeboardTP0 "110073")))
	(if (= DisDifP0 1500) (progn(setq ElToeboardTP0 "110160")))
	(if (= DisDifP0 2000) (progn(setq ElToeboardTP0 "110176")))
	(if (= DisDifP0 2500) (progn(setq ElToeboardTP0 "110208")))
    (if (= DisDifP0 3000) (progn(setq ElToeboardTP0 "110211")))


 	(setq verticalheightTypeL VElevation)
	(SETQ pt1hu pt1VG)
	(if (= exit135 1) 
         (setq typeLSave 1) 		 
		 (setq typeLSave 0) 		 
    )
		(if (= exit180 1)
		(setq TypeV 1)
		(setq TypeV 2)
		)
		(c:InsertHorizontall)
	(setq verticalheightTypeL VElevation)
    (setq pt1hu(polar pt1VG ang0 DistHorizontal))
	(if (= exit45 1) 
         (setq typeLSave 1) 		 
		 (setq typeLSave 0) 		 
    )
		(if (= exit0 1)
		(setq TypeV 1)
		(setq TypeV 2)
		)

 	(c:InsertHorizontalll)	
	(if (= EsScale "1")
		(progn
			(setq verticalheightTypeL VElevation)
			(SETQ pt1Es pt1VG)
			(c:InsertEsc)
		)
	)
	(if (= EsScale "2")
		(progn
			(setq verticalheightTypeL VElevation)
			(SETQ pt1Es pt1VG)
			(c:InsertEscTowerparallel)
		)
	)
	(if (= ExitModAccessDecks "1")
		(progn
			(setq verticalheightTypeL VElevation)
			(SETQ pt1Es pt1VG)
			(c:SETModAccessDecks)
		)
	)
)


(defun c:SETModAccessDecks() 
(setq Mmirror "1")
 (setq NEsc(/ VElevation 2000))
	 (setq Nesc(fix NEsc))
	 (setq LNesc (* Nesc 2000))
	 (setq distRestaEsca(- VElevation LNesc))
(IF (= Disthorizontal 2500)
(SETQ BAccessDecks "114825")
(SETQ BAccessDecks "114812")
)
(SETQ HElvBase HElvBaseD)
(SETQ PP11 (CAR pt1Es))
(SETQ PP22 (CADR pt1Es))
(SETQ PP33 (+ HElvBase 2000)) 
(setq MidelDistance(/ Disthorizontal 2))
(setq ptMirror1 (polar pt1Es ang0  MidelDistance))
(setq ptMirror2 (polar ptMirror1 ang90  100))
(SETQ PP22 (+ PP22 200))
 	(while (> Nesc 0)
			(setq HElvBase(+ HElvBase 2000))
			(SETQ PP33 HElvBase)
			(SET_Insertblock BAccessDecks PP11 PP22 PP33 ang0D 3 "false") 						
			
			(if (= Mmirror "2")
						(progn
							(setq Mmirror "1")
							 (VL-CMDF "_MIRROR" "_L" "" ptMirror1 ptMirror2 "_YES")	
						)
						(progn
							(setq Mmirror "2")
						)
					)
	(setq Nesc(- Nesc 1))
   )
)


(defun c:InsertEscTowerparallel()
(setq VElevation(atof Elevation)) 
(IF (= Disthorizontal 2500)
(progn
(SETQ BStaircase "ModEscS12500")
(SETQ BStaircaseS "ModEscS1S2500")
)
(progn
(SETQ BStaircase "ModEscS13000")
(SETQ BStaircaseS "ModEscS1S3000")
)
)
(setq Nesc(/ VElevation 2000))
	 (setq Nesc(fix NEsc))
	 (setq LNesc (* Nesc 2000))
	 (setq distRestaEsca(- VElevation LNesc))
(SETQ HElvBase HElvBaseD)
(SETQ PP11 (CAR pt1Es))
(SETQ PP22 (CADR pt1Es))

(SETQ PP33 HElvBase)
(SETQ PPG11 (+ PP11 620))
			(SETQ HElvBaseEs (- HElvBase 215))
		(while (> Nesc 0)
            (SETQ PP33 HElvBaseEs)
			(SET_Insertblock BStaircase PP11 PP22 PP33 ang0D 3 "true") 
			(setq Nesc(- Nesc 1))
			(setq HElvBaseEs(+ HElvBaseEs 2000))
		 )
)


(defun c:InsertEsc()
(setq VElevation(atof Elevation)) 
(setq Mmirror "1")
(IF (= Disthorizontal 2500)
(progn
(SETQ BStaircase "ModEscS12500")
(SETQ BStaircaseS "ModEscS1S2500")
)
(progn
(SETQ BStaircase "ModEscS13000")
(SETQ BStaircaseS "ModEscS1S3000")
)
)
(setq Nesc(/ VElevation 2000))
	 (setq Nesc(fix NEsc))
	 (setq LNesc (* Nesc 2000))
	 (setq distRestaEsca(- VElevation LNesc))
(SETQ HElvBase HElvBaseD)
(SETQ PP11 (CAR pt1Es))
(SETQ PP22 (CADR pt1Es))

(SETQ PP33 HElvBase)
(SETQ PPG11 (+ PP11 620))
 (SETQ HElvBaseEs (- HElvBase 215))
	  (while (> Nesc 0)
			(SETQ PP33 HElvBaseEs)
			(if (= Mmirror "1")
						(progn
							(setq Mmirror "2")
							(SET_Insertblock BStaircase PP11 PP22 PP33 ang0D 3 "true") 
						)
						(progn
							(setq Mmirror "1")
							(SET_Insertblock BStaircaseS PP11 PP22 PP33 ang0D 3 "true")
						)
		   )
			(setq Nesc(- Nesc 1))
			(setq HElvBaseEs(+ HElvBaseEs 2000))
		 )
)
 
 (defun c:InsertHorizontalll() 
(SETQ HElvBase HElvBaseD)
(setq NumberOfPlatformsv1 NumberOfPlatforms)
(setq NpLT NpLTD)
(while (> NumberOfPlatformsv1 0)
		(setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
			(SETQ PP11 (CAR pt1hu))
			(SETQ PP22 (CADR pt1hu))
			(SETQ PP33 HElvBase)
				(if (= TypeDifP0 "1")
						  (progn
							  (setq ElHorizontalTR ElHorizontalTRP0)							  
							  (setq ElToeboardT ElToeboardTP0)
							  (setq ElHorizontalT ElHorizontalTP0)					 
						 )
					 )
				(if (= TYpeH "Y")
				   (progn
						(if (= TypeV 2)
									(progn
										(SET_Insertblock ElToeboardT PP11 PP22 PP33 ang90D 3 "false") 						
										(SET_Insertblock ElToeboardT PP11 PP22 PP33 ang90D 3 "false") 						
										(setq HElvBase(+ HElvBase 500))
										(SETQ PP33 HElvBase)
										(SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 						
										(setq HElvBase(+ HElvBase 500))
										(SETQ PP33 HElvBase)
											(SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 						
										(setq HElvBase(+ HElvBase 1000))
									)
									(progn
										(SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 						
										(if (= TypeDifP0 "1")
										 (progn
											(SETQ PP22DIFF(+ PP22 DisDifP0))
											(SET_Insertblock ElHorizontalTDF PP11 PP22DIFF PP33 ang90D 3 "false") 	
										)
										)
										(setq HElvBase(+ HElvBase 2000))
									
									)
						);;end typev 2
						  (if (= TypeDifP0 "1")
							  (progn
								(SETQ PP33(+ PP33 500))
								(SETQ PP22DIFF(+ PP22 DisDifP0))
				(SET_Insertblock ElHorizontalTDF PP11 PP22DIFF PP33 ang90D 3 "false")  				
				(SETQ PP33(+ PP33 500))
				(SET_Insertblock ElHorizontalTDF PP11 PP22DIFF PP33 ang90D 3 "false") 
					))
				 )

				(progn
			(IF (/= NumberOfPlatformsv1 0)
						  (progn
						   (SET_Insertblock ElDiagonalT PP11 PP22 PP33 ang90D 3 "false") 						
						  )
						)
			(SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 						
			(if (= TypeDifP0 "1")
						  (progn
							(SETQ PP22DIFF(+ PP22 DisDifP0))
							(SET_Insertblock ElHorizontalTDF PP11 PP22DIFF PP33 ang90D 3 "false") 						
					))
				   (setq HElvBase(+ HElvBase 2000))
				  );;fin progn
				)
				
 )
  );;end InsertHorizontalll
(defun c:InsertHorizontall()
(SETQ HElvBase HElvBaseD)
(setq NumberOfPlatformsv1 NumberOfPlatforms)
(setq NpLT NpLTD)
;;;;;(print "J")
(if (= TypeV 2)
	   (progn
			 (while (> NumberOfPlatformsv1 0)
			 (setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				(if (= TYpeH "Y")
				   (progn
				   	  (SET_Insertblock ElHorizontalTR PP11 PP22 PP33 ang90D 3 "false") 						
				   	  (SET_Insertblock ElToeboardT PP11 PP22 PP33 ang90D 3 "false") 						
					    (setq HElvBase(+ HElvBase 500))
						(SETQ PP33 HElvBase)
				   	    (SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 						
						(setq HElvBase(+ HElvBase 500))
						(SETQ PP33 HElvBase)
				   	    (SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 						
						(setq HElvBase(+ HElvBase 1000))
					)
				   (progn
				   (IF (/= NumberOfPlatformsv1 0)
					   (progn
					   	(SET_Insertblock ElDiagonalT PP11 PP22 PP33 ang90D 3 "false") 						
					   )
				   )
					(SET_Insertblock ElHorizontalT PP11 PP22 PP33 ang90D 3 "false") 										   
				    (setq HElvBase(+ HElvBase 2000))
				   )
				)
			)  
          )
 )
  
  );;end InsertHorizontall  
(defun c:InsertHorizontal()

(SETQ HElvBase HElvBaseD)
(setq NumberOfPlatformsv1 NumberOfPlatforms)
(setq NpLT NpLTD)
;;;;;(print TypeV)
;;;;;(print "este")
(if (= TypeV 2)
	   (progn
	   ;;;;;(print "4")
			 (while (> NumberOfPlatformsv1 0)
			 ;;;;;(print "3")
			 (setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				(SETQ PH1 (LIST PP11 PP22 PP33))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 						
					(IF (/= NumberOfPlatformsv1 0)
						(progn
							(SET_Insertblock ElDiagonal PP11 PP22 PP33 ang0D 3 "false") 						
						)
					)
					 ;;;;;(print "6")
					(if (= TYpeH "Y")
					   (progn
					    ;;;;;(print "7")
				   ;;esto se puede hacer mejor cambiar despues de la presentacion
									  (if (= EsScaPlat "1")
											(progn
												(SETQ PPL1(POLAR PH1 ANG90 1650))
												(Setq DisPlat (- DisPlat 1500))
											)
											(progn
											 (if (= EsScaPlat "2")
												(progn
												(SETQ PPL1(POLAR PH1 ANG90 875))
												(Setq DisPlat (- DisPlat 750))
												)
												(progn
														(SET_Insertblock ElToeboard PP11 PP22 PP33 ang0D 3 "false") 						
														 (SETQ PPL1(POLAR PH1 ANG90 125))
											  ;; a esto me refiero
														(if (= ExitModAccessDecks "1")
															(progn
																(SETQ PPL1(POLAR PH1 ANG90 900))
																(Setq DisPlat (- DisPlat 750))
															)
															(progn
															;;;;;(print "18")
															(SET_Insertblock ElToeboard PP11 PP22 PP33 ang0D 3 "false") 						
															 (SETQ PPL1(POLAR PH1 ANG90 125))
															;;;;;(print "19")
															)
														)
													)
										 )
                                       )
									   )
										(SETQ PP33l (+ HElvBase 35))
										(while (> DisPlat 0)
											 (SETQ PP11l (CAR PPL1))
						   					 (SETQ PP22l (CADR PPL1))
											 (SET_Insertblock ElPlat PP11l PP22l PP33l ang90D 3 "false") 						
											 ;;;;;;(printang90D)
											 (SETQ PPL1(POLAR PPL1 ANG90 250))
											 (SETQ DisPlat(- DisPlat 250))
										)
										(setq HElvBase(+ HElvBase 500))
										(SETQ PP33 HElvBase)
										(SETQ PH1 (LIST PP11 PP22 PP33))
										(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
										(setq HElvBase(+ HElvBase 500))
										(SETQ PP33 HElvBase)
										(SETQ PH1 (LIST PP11 PP22 PP33))
										(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
										(setq HElvBase(+ HElvBase 1000))
					)
				   (progn
				    ;;;;;(print "8")
				   (setq HElvBase(+ HElvBase 2000))
				   )
				 
				)
			;;;;;(print "10")
			)  
          ;;;;;(print "11")
		  )
		(progn
			 (while (> NumberOfPlatformsv1 0)
			;;;;;(print "1")
			(setq DisPlat DistVertical)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				(SETQ PH1 (LIST PP11 PP22 PP33))
				(if (= TYpeH "Y")
				   (progn
				       (SETQ PP33l (+ PP33 35))
					   (SETQ PPL1(POLAR PH1 ANG90 125))
							(while (> DisPlat 0)
							(SETQ PP11 (CAR PPL1))
							(SETQ PP22 (CADR PPL1))
							(SETQ PP33 HElvBase)
							 (SET_Insertblock ElPlat PP11 PP22 PP33l ang90D 3 "false") 
							 (SETQ PPL1(POLAR PPL1 ANG90 250))
							 (SETQ DisPlat(- DisPlat 250))
							)
					     (setq HElvBase(+ HElvBase 2000))
					)
				   (progn
				   (setq HElvBase(+ HElvBase 2000))
				   )
				)
			)  
          )
 )
  );;end InsertHorizontal

  (defun c:InsertHorizontal2()
  (SETQ HElvBase HElvBaseD)
  (setq NumberOfPlatformsv1 NumberOfPlatforms)
  (setq NpLT NpLTD)
			 (while (> NumberOfPlatformsv1 0)
			  (SETQ TNpLT(itoa NpLT))
			  (SETQ TT (vl-string-position (ascii TNpLT)DecksLevels))
			  (SETQ TT (+ TT 2))
			  (SETQ TYpeH(substr DecksLevels TT 1))
			  (setq NumberOfPlatformsv1(- NumberOfPlatformsv1 1))
			  (setq NpLT (+ NpLT 1))
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				(SETQ PH1 (LIST PP11 PP22 PP33))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang18D 3 "false") 
				;;;;;(print "Tes1")
				(if (= TYpeH "Y")
				   (progn
						(IF (= TypeV 1)
				         (progn
								;;;   (IF (/= NumberOfPlatformsv1 0)
								;;; 	 (progn
									;;;;;;   (VL-CMDF "_insert" ElToeboard PH1  "" "" ang18D) 
								;;; 	   (setq HElvBase(+ HElvBase 500))
								;;; 		(SETQ PP33 HElvBase)
									;;;;; 	(VL-CMDF "_insert" ElHorizontal PH1  "" "" ang18D) 
								;;; 		(setq HElvBase(+ HElvBase 500))
								;;; 		(SETQ PP33 HElvBase)
									;;;;; 	(VL-CMDF "_insert" ElHorizontal PH1  "" "" ang18D)  
								;;; 		(setq HElvBase(+ HElvBase 1000))
								;;;    )
								;;;   )
						  )
						 (progn
						 ;;MIRAR si inserto verticales
						 (setq InsertElementH2 0)
						  (IF (= ModBracketPlatform "1")
						   (progn
 							   (setq InsertElementH2 (+ InsertElementH2 1))
							)
						  )
						  (IF (= ModPSDS1 "1")
						   (progn
							   (setq InsertElementH2 (+ InsertElementH2 1))
							)
					      )
							    (IF (/= NumberOfPlatformsv1 0)
									(progn
										(if (= InsertElementH2 0)
											(progn 
												(SET_Insertblock ElDiagonal PP11 PP22 PP33 ang18D 3 "false") 
												 
											)
										)
									)
								)
								
								(if (= InsertElementH2 0)
									(progn 
										(SET_Insertblock ElToeboard PP11 PP22 PP33 ang18D 3 "false")
													 
									)
								)
							    (setq HElvBase(+ HElvBase 500))
								(SETQ PP33 HElvBase)
								(if (= InsertElementH2 0)
									(progn  
									(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang18D 3 "false")
													 
									)
								) 
								(setq HElvBase(+ HElvBase 500))
								(SETQ PP33 HElvBase)
								(if (= InsertElementH2 0)
									(progn  
									(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang18D 3 "false")
									   
									)
								)  
								(setq HElvBase(+ HElvBase 1000))
						 )
					   );;if Type V1
					)
					
				   (progn
					   (IF (/= NumberOfPlatformsv1 0)
						  (progn
						  (SET_Insertblock ElDiagonal PP11 PP22 PP33 ang18D 3 "false") 
						)
						)
				   (setq HElvBase(+ HElvBase 2000))
				   )
		 );;end if TYpeH "S"
	 );; End while
  );;end InsertHorizontal
 

(defun c:ManagerVerticalModuleAccessLadder() 
(c:ManagerVertical)
;;	(if (and (= exit180 1) 
;;			 (= exit225 1) 
;;			 (= exit270 1)
;;			 )
;;		(setq TypeV 0)
;;		(setq TypeV 1)
;;		)
;;		(SETQ TypeS1 "0")
;;
;;	(c:InsertVertical)
;;
;;	;Vert pt90 
;;	  (if (and (= exit180 1) 
;;			   (= exit135 1) 
;;			   (= exit90 1)
;;		   )
;;				(setq TypeV 0)
;;				(setq TypeV 1)
;;		)
;;	  (setq pt1(polar pt1VG ang90 DistVertical))
;;	  (if (> DistMensuTrav 1499)
;;				(progn
;;				(SETQ TypeS1 "1")
;;				)
;;	  )
;;	  (if (/= exit90 "1")(c:InsertVertical))
;;	  (if (and (= exit90 1) 
;;			   (= exit45 1) 
;;			   (= exit0 1)
;;		   )
;;				(setq TypeV 0)
;;				(setq TypeV 1)
;;		)
;;	  (setq pt1(polar pt1VG ang0  DistHorizontal))
;;	   (setq pt1(polar pt1 ang90  DistVertical))
;;
;;	 (if (/= exit90 1) 
;;		(progn
;;			(if (/= exit180 1) 
;;			  (progn
;;				(c:InsertVertical)  
;;			  )
;;		   )
;;		)
;;		)
;;	  ;Vert pt180 
;;	  (if (and (= exit0 1) 
;;			   (= exit315 1) 
;;			   (= exit270 1)
;;		   )
;;				(setq TypeV 0)
;;				(setq TypeV 1)
;;		)
;;	  (setq pt1(polar pt1VG ang0  DistHorizontal))
;;	 (SETQ TypeS1 "0")
;;
;;	 (if (/= exit180 1)(c:InsertVertical))
 

 (setq DistVertical750(+ DistVertical 750))
 (setq pt1(polar pt1VG ang90 DistVertical))
 (setq pt1Ladder(polar pt1VG ang90 DistVertical750))
 (setq pt1ref pt1)
 (c:InsertVerticalLadder)  
) 


(defun c:InsertVerticalLadder ()
    (setq NSetVCont40Lad NSetVCont40La)
	(setq NSetVCont30Lad NSetVCont30La)
	(setq NSetVCont20Lad NSetVCont20La)
	(setq ELvBaseL ELvBase)
	(setq PTL PT1)
	 (SETQ PP11 (CAR PTL))
	 (SETQ PP22 (CADR PTL))
     (setq ELvBaseL (+ ELvBaseL 1740))
	 (setq nModi 0)
				(while (> NSetVCont40Lad 0)
					    (SETQ PP33 ELvBaseL)
						(if (= nModi 0)
						 (progn
							(SET_Insertblock "AccL40000i" PP11 PP22 PP33 ang0D 3 "True") 
							(setq nModi 1)
						 )
						 (progn
							(SET_Insertblock "AccL40000" PP11 PP22 PP33 ang0D 3 "True") 
						 )
						)
						(setq NSetVCont40Lad(- NSetVCont40Lad 1))
						(setq ELvBaseL (+ ELvBaseL 4000))
				 )
				(while (> NSetVCont30Lad 0)
				(SETQ PP11 (CAR PTL))
				  (SETQ PP33 ELvBaseL)
					(if (= nModi 0)
						 (progn
							(SET_Insertblock "AccL3000i" PP11 PP22 PP33 ang0D 3 "True") 
						 	(setq nModi 1)
						 )
						 (progn
							(SET_Insertblock "AccL3000" PP11 PP22 PP33 ang0D 3 "True") 
						 )
					)
						(setq NSetVCont30Lad(- NSetVCont30Lad 1))
						(setq ELvBaseL (+ ELvBaseL 3000))
				 )
        
				(while (> NSetVCont20Lad 0)
				;;;;;(print NSetVCont20)
				(SETQ PP33 ELvBaseL)
					(if (= nModi 0)
						 (progn
							(SET_Insertblock "AccL2000i" PP11 PP22 PP33 ang0D 3 "True") 
							(setq nModi 1)
						)
						 (progn
							(SET_Insertblock "AccL2000" PP11 PP22 PP33 ang0D 3 "True") 
						 )
					)
							  	  
								  (setq NSetVCont20Lad(- NSetVCont20Lad 1))
								  (setq ELvBaseL (+ ELvBaseL 2000))
				)
			 
				(if (= NSetVCont15La  1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "AccL1500" PP11 PP22 PP33 ang0D 3 "True") 
		  						  (setq ELvBaseL (+ ELvBaseL 1500))
				 ))
				 	

				(if (= NSetVCont10La  1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "AccL1000" PP11 PP22 PP33 ang0D 3 "True") 						
		  						  (setq ELvBaseL (+ ELvBaseL 1000))
				 ))
				 
				(if (= NSetVCont05La  1)(progn 
					              (SETQ PP33 ELvBaseL)
								  (SET_Insertblock "AccL500" PP11 PP22 PP33 ang0D 3 "True") 						
								  (setq ELvBaseL (+ ELvBaseL 500))
				 ))

	 (SETQ PP11 (CAR PTL))
	 (SETQ PP22 (CADR PTL))
	 (SETQ PP11 (+ PP11 DistHorizontal))
     (setq ELvBaseL (+ ELvBase 1500))
	 (SETQ PP33 (+ ELvBaseL 120))
	 (SET_Insertblock "112678" PP11 PP22 PP33 ang90D 3 "false") 	
 		(if (= DistHorizontal 1500)(progn(setq ElHorizontalTR "114681")))
		(if (= DistHorizontal 2000)(progn(setq ElHorizontalTR "114687")))
		(if (= DistHorizontal 2500)(progn(setq ElHorizontalTR "114691")))
		(if (= DistHorizontal 3000)(progn(setq ElHorizontalTR "114695")))
		 (SETQ PP11 (CAR PTL))
		 (SETQ PP22 (CADR PTL))
		 (SET_Insertblock ElHorizontalTR PP11 PP22 PP33 ang0D 3 "false") 
		 (SETQ PP22 (+ PP22 750))
 		 (SET_Insertblock ElHorizontalTR PP11 PP22 PP33 ang0D 3 "false") 


		 
	 (SETQ PP33  (- ELvBase 140))
	 (SETQ PP11 (CAR Pt1))
	 (SETQ PP22 (CADR Pt1))
	 (SET_Insertblock "AccLEsci" PP11 PP22 PP33 ang0D 3 "false") 
	 

	 (setq VElevationd(atof Elevation)) 
	 (setq VElevationd(- VElevationd 1828)) 
	 (while (> VElevationd 0)
	 (SETQ PP33 (+ PP33 1828))
	  (SET_Insertblock "AccLEsc" PP11 PP22 PP33 ang0D 3 "false") 
	  (setq VElevationd(- VElevationd 1828))
	  )
	   
	 (setq VElevationd(atof Elevation)) 
	 (setq VElevationd(- VElevationd 3570)) 
	 (SETQ PP33  (+ ELvBase 3570))
	(while (> VElevationd 0)
	  (SET_Insertblock "AccLEsct" PP11 PP22 PP33 ang0D 3 "false") 
	  (SETQ PP33  (+ PP33 1000))
	  (setq VElevationd(- VElevationd 1000))
	  )

 )

  
(defun c:ManagerVerticalStaircase100125() 
	(setq TypeV 1)
	(SETQ TypeS1 "0")
(c:InsertVertical)

(setq pt1(polar pt1VG ang90 DistVertical))
	(setq TypeV 1)
	(SETQ TypeS1 "0")
(c:InsertVertical)
(setq TypeV 1)
	(SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang0  DistHorizontal))
   (setq pt1(polar pt1 ang90  DistVertical))
(c:InsertVertical)
(setq TypeV 1)
(SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang0  DistHorizontal))
  (c:InsertVertical)
  ;;Punto 5
  (setq TypeV 1)
  (SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang0 3500))
  (c:InsertVertical)
  ;;Punto 6
  (setq TypeV 1)
  (SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang0  3500))
  (setq pt1(polar pt1 ang90  DistVertical))
  (c:InsertVertical)
   ;;Punto 7
  (setq TypeV 1)
  (SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang18  1000))
  (c:InsertVertical)
   ;;Punto 8
  (setq TypeV 1)
  (SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang18  1000))
  (setq pt1(polar pt1 ang90  DistVertical))
  (c:InsertVertical)
   ;;Punto 11
  (setq TypeV 1)
  (SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang90 1000))
  (c:InsertVertical)
   ;;Punto 22
  (setq TypeV 1)
  (SETQ TypeS1 "0")
  (setq pt1(polar pt1VG ang90 1000))
  (setq pt1(polar pt1 ang0  DistHorizontal))
  (c:InsertVertical)
 
) 



(defun c:ManagerHorizontalStaircase100125()
	(setq NpLTD 1) 
	(setq TypeV 0)
	(if (= DistHorizontal 720) (progn(setq ElHorizontal "114124")))
	(if (= DistHorizontal 750) (progn(setq ElHorizontal "114629")))
	(if (= DistHorizontal 1000)(progn(setq ElHorizontal "114632")))
	(if (= DistHorizontal 1250)(progn(setq ElHorizontal "114638")))
	(if (= DistHorizontal 1500)(progn(setq ElHorizontal "114641")))
	(if (= DistHorizontal 2000)(progn(setq ElHorizontal "114645")))
	(if (= DistHorizontal 2500)(progn(setq ElHorizontal "114648")))
	(if (= DistHorizontal 3000)(progn(setq ElHorizontal "114651")))

	(setq  ElHorizontalL "114645")
    (if (= UseUHPlus "True")(progn(setq ElHorizontalL "114687")))

	(if (= DistHorizontal 720) (progn(setq ElDiagonal "114124")))
	(if (= DistHorizontal 750) (progn(setq ElDiagonal "124170")))
	(if (= DistHorizontal 1000)(progn(setq ElDiagonal "112926")))
	(if (= DistHorizontal 1250)(progn(setq ElDiagonal "114638")))
	(if (= DistHorizontal 1500)(progn(setq ElDiagonal "100572")))
	(if (= DistHorizontal 2000)(progn(setq ElDiagonal "100573")))
	(if (= DistHorizontal 2500)(progn(setq ElDiagonal "100574")))
	(if (= DistHorizontal 3000)(progn(setq ElDiagonal "100575")))
 
    (if (= DistHorizontal 750)  (progn(setq ElToeboard "110213")))
	(if (= DistHorizontal 1000) (progn(setq ElToeboard "110073")))
	(if (= DistHorizontal 1500) (progn(setq ElToeboard "110160")))
	(if (= DistHorizontal 2000) (progn(setq ElToeboard "110176")))
	(if (= DistHorizontal 2500) (progn(setq ElToeboard "110208")))
    (if (= DistHorizontal 3000) (progn(setq ElToeboard "110211")))

    (if (= DistHorizontal 750)   (progn(setq ElPlat "124121")))
    (if (= DistHorizontal 1000)  (progn(setq ElPlat "124118")))
    (if (= DistHorizontal 1500)  (progn(setq ElPlat "124112")))
    (if (= DistHorizontal 2000)  (progn(setq ElPlat "124109")))
    (if (= DistHorizontal 2500)  (progn(setq ElPlat "123771")))
    (if (= DistHorizontal 3000)  (progn(setq ElPlat "124915")))
	(setq verticalheightTypeL VElevation)
	(SETQ pt1hu pt1VG)
	(if (and (= exit180 1) 
			 (= exit225 1) 
			 (= exit270 1)
	    ) 
		(setq Type1V 0)
		(setq Type1V 1)
	)
	(if (and (= exit0 1) 
             (= exit315 1) 
             (= exit270 1)
	    )
			(setq Type2V 0)
			(setq Type2V 1)
	)
	(setq TypeV (+ Type2V Type1V))
		(if (= exit270 1)
		(setq TypeV 1)
		(setq TypeV 2)
	    )
 	
	(c:InsertHorizontalStaircase100125)

)

(defun c:InsertHorizontalStaircase100125()

(SETQ HElvBase HElvBaseD)
(SETQ PP11 (CAR pt1hu))
(SETQ PP22 (CADR pt1hu))
(setq VElevation(atof Elevation)) 
(setq NModul44(float VElevation))
(SETQ NModul4(/ NModul44 4000))
(setq NModul4d(fix NModul4))
(setq Restod(* NModul4d 4000))
(setq Resto(- NModul44 Restod))
	(while (> NModul4d 0);;cominenza while
	;;;;;(print "4")
				(SETQ PP33 HElvBase)
				;;ladoLateral
				;;lado forntal
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(- PP11 3500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				
				;;Quitamiedos superior
				(setq pp33l(+ pp33 500))
				(SETQ PP22l (+ PP22 2000))
				(SET_Insertblock "114632" PP11 PP22 PP33l ang0D 3 "false") 
				(SET_Insertblock "114632" PP11 PP22l PP33l ang0D 3 "false") 
				(setq pp33l(+ pp33l 500))
				(SET_Insertblock "114632" PP11 PP22 PP33l ang0D 3 "false") 
				(SET_Insertblock "114632" PP11 PP22l PP33l ang0D 3 "false") 
				(SETQ PP22 (CADR pt1hu))
				;;lado atras
				;;;;;(print "2")
				(SETQ PP22 (+ PP22 2000))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 1000))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;Lado Medio
				;;;;;(print "3")
				(SETQ PP22 (- PP22 1000))
				(SETQ PP11(- PP11 2500))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				;;Plataformas
				(SETQ PP22 (- PP22 1000))
				(SETQ PP11(- PP11 875))
				(setq PP33pl(+ PP33 35))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				(SETQ PP11(+ PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				(SETQ PP11(+ PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				(SETQ PP11(+ PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				;;Diagonales
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(setq pp11s(polar pt1hu ang90 500))
				(setq pp22s(polar pp11s ang0 500))
				(setq PP33(+ PP33 500))
				(SET_Insertblock "1005749" PP11 PP22 PP33 ang0D 3 "false") 
			   
				(setq pp22s(+ PP22 1000))
				(SET_Insertblock "100574" PP11 pp22s PP33 ang0D 3 "false") 
				;;aqi simetria
				(setq PP33(+ PP33 500))
				(SET_Insertblock "1005749" PP11 PP22 PP33 ang0D 3 "false") 
				(SET_Insertblock "100574" PP11 pp22s PP33 ang0D 3 "false") 
				;;(VL-CMDF "_MIRROR" "_L" "" pp11s pp22s "_No")
				(setq PP33(- PP33 1000))
				;;EscaleraInicial
				(SET_Insertblock "1092199" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP22 (+ pp22 1000))
				(SET_Insertblock "1092199" PP11 PP22 PP33 ang0D 3 "false") 
			;;	(VL-CMDF "_MIRROR" "_L" "" pt1hu pp11s "_Yes")
						;;Planta Escalones
							(SETQ PP33Esc (- PP33 80))
							(SETQ PP11 (CAR pt1hu))
							(SETQ PP22 (CADR pt1hu))
							(SETQ PP22 (+ PP22 1000))
							(SET_Insertblock "109198" PP11 PP22 PP33Esc ang27D 3 "false") 			
						(setq ContE 9)
						(while (> ContE 0)
							(SETQ PP11 (+ PP11 250))
							(setq PP33Esc(+ PP33Esc 200))
							(SET_Insertblock "109198" PP11 PP22 PP33Esc ang27D 3 "false") 			
						(setq ContE(- ContE 1))
					  ) 	   
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP11 (- PP11 1000))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				;;Lateral
				 
				(SET_Insertblock ElHorizontalL PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 500))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 500))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 1000))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				;;Insert Diagonal Lateral
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				(SETQ PP11 (+  PP11 3500))
				(SETQ PP22 (+ PP22 2000))
				(SET_Insertblock "100573" PP11 PP22 PP33 ang27D 3 "false")  
			;;Planta Superior
				(SETQ PP33 (+ HElvBase 2000))
				;;ladoLateral2
				;;lado forntal
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(- PP11 3500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;;;;(print "5")
			    ;;Quitamiedos superior
				(SETQ PP11l (CAR pt1hu))
				(setq PP11l(+ PP11l 2500))
				(setq PP33l(+ PP33 500))
				(SETQ PP22l (+ PP22 2000))
				;;;;;(print "6")
				(SET_Insertblock "114632" PP11l PP22 PP33l ang0D 3 "false") 
				(SET_Insertblock "114632" PP11l PP22l PP33l ang0D 3 "false") 
				(setq pp33l(+ pp33l 500))
				(SET_Insertblock "114632" PP11l PP22 PP33l ang0D 3 "false") 
				(SET_Insertblock "114632" PP11l PP22l PP33l ang0D 3 "false") 
				
				;;lado atras
				(SETQ PP22 (+ PP22 2000))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 1000))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;Lado Medio
				(SETQ PP22 (- PP22 1000))
				(SETQ PP11(- PP11 2500))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false")
				;;Plataformas
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP22 (+ PP22 2000))
				(SETQ PP11(+ PP11 3375))
				(setq PP33pl(+ PP33 35))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP11(- PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP11(- PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP11(- PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				;;EscaleraFinal
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(setq PP11 (+ PP11 2500))
				(SETQ PP22(+ PP22 1000))
				(SET_Insertblock "109219" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP22 (+ pp22 1000))
				(SET_Insertblock "109219" PP11 PP22 PP33 ang0D 3 "false") 
						;;Planta Escalones
							(SETQ PP33Esc (- PP33 80))
							(SETQ PP22 (- PP22 1000))
							(SET_Insertblock "109198" PP11 PP22 PP33Esc ang90D 3 "false") 			
						(setq ContE 9)
						(while (> ContE 0)
							(SETQ PP11 (- PP11 250))
							(setq PP33Esc(+ PP33Esc 200))
							(SET_Insertblock "109198" PP11 PP22 PP33Esc ang90D 3 "false") 			
						(setq ContE(- ContE 1))
					  )
				 ;;Lateral2
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP11 (+ PP11 3500))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 (+ HElvBase 2000))
				(SET_Insertblock ElHorizontalL PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 500))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 500))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 1000))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				;;Insert Diagonal Lateral
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP11 (-  PP11 1000))
				(SETQ PP33 (- PP33 2000))
				(SET_Insertblock "100573" PP11 PP22 PP33 ang90D 3 "false") 
				;;Diagonales superior 
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(setq pp11s(polar pt1hu ang90 1500))
				(setq pp22s(polar pp11s ang0 1500))
				(SETQ PP22 (+ PP22 1000))
				(SETQ PP11 (+ PP11 2500))
				(setq PP33(+ PP33 500))
				(SET_Insertblock "100574" PP11 PP22 PP33 ang18D 3 "false")
				(SETQ PP22D (+ PP22 1000))
				(SET_Insertblock "1005749" PP11 PP22D PP33 ang18D 3 "false")
				(setq PP33(+ PP33 500))
				(SET_Insertblock "100574" PP11 PP22 PP33 ang18D 3 "false")
				(SET_Insertblock "1005749" PP11 PP22D PP33 ang18D 3 "false")
				(setq PP33(- PP33 1000))
				(setq NModul4d(- NModul4d  1))
				(SETQ HElvBase(+ HElvBase 4000))
			 
	);;Finaliza while
	;;el 10 hay que cambiarlo
	(if (> Resto 10)(progn(C:InsertHorizontalStaircase100125End)))
)
(DEFUN C:InsertHorizontalStaircase100125End()

(SETQ PP33 HElvBase)
				;;ladoLateral
				;;lado forntal
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(- PP11 3500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;lado atras
				(SETQ PP22 (+ PP22 2000))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 1000))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;Lado Medio
				(SETQ PP22 (- PP22 1000))
				(SETQ PP11(- PP11 2500))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				
				(setq PP33(+ PP33 2000))
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(- PP11 3500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;lado atras
				(SETQ PP22 (+ PP22 2000))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 1000))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(SETQ PP11(+ PP11 2500))
				(SET_Insertblock "114632" PP11 PP22 PP33 ang0D 3 "false") 
				;;Lado Medio
				(SETQ PP22 (- PP22 1000))
				(SETQ PP11(- PP11 2500))
				(SET_Insertblock ElHorizontal PP11 PP22 PP33 ang0D 3 "false") 
				(setq PP33(- PP33 2000))
 				;;Plataformas
				(SETQ PP22 (- PP22 1000))
				(SETQ PP11(- PP11 875))
				(setq PP33pl(+ PP33 35))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				(SETQ PP11(+ PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				(SETQ PP11(+ PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				(SETQ PP11(+ PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang18D 3 "false") 
				;;Diagonales
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(setq pp11s(polar pt1hu ang90 500))
				(setq pp22s(polar pp11s ang0 500))
				(setq PP33(+ PP33 500))
				(SET_Insertblock "1005749" PP11 PP22 PP33 ang0D 3 "false") 
				(setq PP33(+ PP33 500))
				(SET_Insertblock "1005749" PP11 PP22 PP33 ang0D 3 "false") 
				(setq PP33(- PP33 1000))
				;;EscaleraInicial
				(SET_Insertblock "1092199" PP11 PP22 PP33 ang0D 3 "false") 
				(setq pp11s(polar pt1hu ang90 500))
				(SETQ PP22 (+ pp22 1000))
				(SET_Insertblock "1092199" PP11 PP22 PP33 ang0D 3 "false") 
						;;Planta Escalones
							(SETQ PP33Esc (- PP33 80))
							(SETQ PP11 (CAR pt1hu))
							(SETQ PP22 (CADR pt1hu))
							(SETQ PP22 (+ PP22 1000))
							(SET_Insertblock "109198" PP11 PP22 PP33Esc ang27D 3 "false") 			
						(setq ContE 9)
						(while (> ContE 0)
							(SETQ PP11 (+ PP11 250))
							(setq PP33Esc(+ PP33Esc 200))
							(SET_Insertblock "109198" PP11 PP22 PP33Esc ang27D 3 "false") 			
						(setq ContE(- ContE 1))
					  ) 	   
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP11 (- PP11 1000))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				;;Lateral
			 
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 500))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 500))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
				(SETQ PP33 (+ PP33 1000))
				(SET_Insertblock "114645" PP11 PP22 PP33 ang90D 3 "false") 			
			
			;;Insert Diagonal Lateral-
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP33 HElvBase)
				(SETQ PP11 (+  PP11 3500))
				(SETQ PP22 (+ PP22 2000))
				(SET_Insertblock "100573" PP11 PP22 PP33 ang27D 3 "false")  
				;;Plataformas
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				(SETQ PP22 (+ PP22 2000))
				(SETQ PP11(+ PP11 3375))
				(setq PP33pl(+ PP33 2070))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP11(- PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP11(- PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP11(- PP11 250))
				(SET_Insertblock "124109" PP11 PP22 PP33pl ang0D 3 "false") 
				(SETQ PP33 (+ HElvBase 2000))
				(SETQ PP11 (CAR pt1hu))
				(SETQ PP22 (CADR pt1hu))
				;;Quitamiedos superior
				(SETQ PP11l (CAR pt1hu))
				(setq PP11l(+ PP11l 2500))
				(setq PP33l(+ PP33 500))
				(SETQ PP22l (+ PP22 2000))
				(SET_Insertblock "114632" PP11l PP22 PP33l ang0D 3 "false") 
			;;	(SET_Insertblock "114632" PP11l PP22l PP33l ang0D 3 "false") 
				(setq pp33l(+ pp33l 500))
				(SET_Insertblock "114632" PP11l PP22 PP33l ang0D 3 "false") 
			;;	(SET_Insertblock "114632" PP11l PP22l PP33l ang0D 3 "false") 
				(SETQ PP33lL(- pp33l 500))
				(setq pp11ll(+ PP11l 1000))
				(SET_Insertblock "114645" pp11ll PP22 PP33lL ang90D 3 "false")
				(SETQ PP33Ll(+ PP33lL 500))
				(SET_Insertblock "114645" pp11ll PP22 PP33Ll ang90D 3 "false")
				(SETQ PP33Ll(- pp33l 500))
				(SETQ pp11llL(- pp11ll 1000))
				(SETQ PP22LL(+ PP22 1000))
				(SET_Insertblock "114632" pp11llL PP22LL PP33Ll ang90D 3 "false")
				(SETQ PP33Ll(+ PP33Ll 500))
				(SET_Insertblock "114632" pp11llL PP22LL PP33Ll ang90D 3 "false")
				(SETQ PP33Ll(- PP33Ll 1000))
				(SET_Insertblock "114632" pp11llL PP22LL PP33Ll ang90D 3 "false")
				(setq pp11ll(+ PP11l 1000))
				(setq PP22LL(- PP22LL 1000))
				(SET_Insertblock ElHorizontalL pp11ll PP22LL PP33Ll ang90D 3 "false")
				;;;;(print "3")
)

(defun c:DefineBlock()
(setq TypeDifP0 "0")
(setq DisDifP0 0)
(setq DisDifP02 0)
(setq TypeDifP0 "0")
(setq DisDifP0 0)
(SETQ PP11 (CAR pt1))
(SETQ PP22 (CADR pt1))
(setq exit0  0)
(setq exit45  0)
(setq exit90  0)
(setq exit135 0)
(setq exit180 0)
(setq exit225 0)
(setq exit270 0)
(setq exit315 0)
(setq ElmentNumberComd 0)
(setq idxCom -1)
  (setq cont0 0)
  (setq okModu "N")  
  (if (setq s (ssget "_X" '((0 . "INSERT") (2 . "`*U*,Module,ModuleStaircase100125,ModAccessDecks,ModBracketPlatform,ModCantilever,ModStairTowerAlternate,ModStairTowerparallel,ModuleAccessLadder"))))
    
   (repeat (setq i (sslength s))
     (setq e (ssname s (setq i (1- i))))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "Module")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModuleStaircase100125")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModAccessDecks")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModBracketPlatform")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModCantilever")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModStairTowerAlternate")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModStairTowerparallel")(setq okModu "y"))
	   (if (= (vla-get-effectivename (vlax-ename->vla-object e)) "ModuleAccessLadder")(setq okModu "y"))
	   (if (= okModu "y")
          (progn
			  (setq lLVCom (entget e))
				  (setq pt1Com (cdr (assoc 10 lLVCom)))
					(setq vlaobjCom (vlax-ename->vla-object e))
					(setq sibloquedCom (vlax-get-property vlaobjCom 'isdynamicblock))
					(if (= sibloquedCom :vlax-true)
				    		(progn
						      (setq variablesCom (vla-getdynamicblockproperties vlaobjCom))
						      (setq valoresCom (vlax-variant-value variablesCom))
						      (setq listaCom (vlax-safearray->list valoresCom))
						      (setq total_valoresCom (length listaCom))
						      (setq contadorCom 0)
						      (setq valor2Com 0)
							     (setq pasowhile1Com 0) 
								 (while (< contadorCom total_valoresCom)
									(setq pasowhile1Com (+ pasowhile1Com 1))
									(setq valorCom (vlax-get-property (nth contadorCom listaCom) "Value"))
										(SETQ valor0Com (vlax-variant-type valorCom))
										(setq valor00Com (vlax-variant-value valorCom))
										;;;(print valor00Com)
										(if(=(vlax-get-property (nth contadorCom listaCom) "PropertyName") "DistVertical")
										   (progn
											  (setq DistVerticaldCom (fix valor00Com))
											  (if(> DistVerticaldCom 745) (setq DistVerticalCom 750))
											  (if(> DistVerticaldCom 995) (setq DistVerticalCom 1000))
											  (if(> DistVerticaldCom 1495)(setq DistVerticalCom 1500))
											  (if(> DistVerticaldCom 1995)(setq DistVerticalCom 2000))
											  (if(> DistVerticaldCom 2495)(setq DistVerticalCom 2500))
											  (if(> DistVerticaldCom 2995)(setq DistVerticalCom 3000))
											  (if(> DistVerticaldCom 3005)(setq DistVerticalCom 0))
										)
				                        )
										(if(=(vlax-get-property (nth contadorCom listaCom) "PropertyName") "DistHorizontal")
										   (progn
											   
											   (setq DistHorizontaldCom (fix valor00Com))
											   (if(> DistHorizontaldCom 745) (setq DistHorizontalCom 750))
											   (if(> DistHorizontaldCom 995) (setq DistHorizontalCom 1000))
											   (if(> DistHorizontaldCom 1495)(setq DistHorizontalCom 1500))
											   (if(> DistHorizontaldCom 1995)(setq DistHorizontalCom 2000))
											   (if(> DistHorizontaldCom 2495)(setq DistHorizontalCom 2500))
											   (if(> DistHorizontaldCom 2995)(setq DistHorizontalCom 3000))
											   (if(> DistHorizontaldCom 3005)(setq DistHorizontalCom 0))
										  )
				                        )
										 ;;;(print DistVerticaldCom)
										;;;(print DistVerticalCom)
									 
							(setq contadorCom (+ 1 contadorCom))
						)
						
					;;Start 0
							(SETQ PP11180 (CAR pt1Com))
							(setq pt1180(- PP11180 DistHorizontalCom))
				            (SETQ PP22180 (CADR pt1Com))
							;(print "pt1com0=")
							;(print PP11180)
							;(print "PP11=")
							;(print PP11)
							;(print "PP22180=")
							;(print PP22180)
							;(print "PP22=")
							;(print PP22)
							(if (and (= (fix pt1180) (fix PP11)) (= (fix PP22180) (fix PP22)))
								(progn
								;(print "2")
									(setq exit0 1)
									(if (/= DistVerticalCom DistVertical)
										(progn
										
											(setq TypeDifP0 "1")
											(setq DisDifP02 (- DistVertical DistVerticalCom))
										  
											(setq DisDifP0 DistVerticalCom)
												 (if (< DisDifP02 0)
												 (progn
													 (setq DisDifP02 (- DistVerticalCom DistVertical))
													 (setq DisDifP0 DistVertical)
												 )
												 )
										)

										 
									)
							 
								)
							)
							
					;;Start 45
							(SETQ PP1145(CAR pt1Com))
				            (SETQ PP2245(CADR pt1Com))
					        (SETQ PP1145(- PP1145 DistHorizontalCom))
							(SETQ PP2245(- PP2245 DistVerticalCom))

							(if (and (= (fix PP1145) (fix PP11)) (= (fix PP2245) (fix PP22)))(setq exit45 1))
						
						;;Start 90
						    (SETQ PP1190 (CAR pt1Com))
							(SETQ PP2290 (CADR pt1Com))
				            (SETQ PP2290 (- PP2290 DistVerticalCom))
							(if (and (= (fix PP1190) (fix PP11)) (= (fix PP2290) (fix PP22)))(progn(setq exit90 1)))
						   ;;Start 135
							(setq pt1135(polar pt1Com ang0 DistHorizontalCom))
							(setq pt1135(polar pt1135 ang27 DistVerticalCom))
						
						    (SETQ PP11135 (CAR pt1Com))
							(SETQ PP22135 (CADR pt1Com))
						    (SETQ PP11135 (+ PP11135 DistHorizontalCom))
						    (SETQ PP22135 (- PP22135 DistVerticalCom))
							(if (and (= (fix PP11135) (fix PP11)) (= (fix PP22135) (fix PP22)))(setq exit135 1))
						  ;;Start 180
						    (setq DistHorizontaldCom(fix DistHorizontalCom))
							;;;;(print DistHorizontaldCom)
						    
							(SETQ pt1180 (CAR pt1Com))
							(setq pt1180(+ pt1180 DistHorizontalCom))
				            (SETQ PP22180 (CADR pt1Com))
							(if (and (= (fix pt1180) (fix PP11)) (= (fix PP22180) (fix PP22)))(setq exit180 1))
						 
						  
						 ;;Start 225
							(SETQ PP11225 (CAR pt1Com))
				            (SETQ PP22225 (CADR pt1Com))
							(SETQ PP11225 (+ PP11225 DistHorizontalCom))
							(SETQ PP22225 (+ PP22225 DistVerticalCom))
							(if (and (= (fix PP11225) (fix PP11)) (= (fix PP22225) (fix PP22)))(setq exit225 1))
						  ;;Start 270
							(SETQ PP11270 (CAR pt1Com))
				            (SETQ PP22270 (CADR pt1Com))
							
				            (SETQ PP22270 (+ PP22270 DistVerticalCom))
							
							(if (and (= PP11270 PP11) (= PP22270 PP22))(setq exit270 1))
						  ;;Start 315
							(SETQ PP11315 (CAR pt1Com))
				            (SETQ PP22315 (CADR pt1Com))
							(SETQ PP11315 (- PP11315 DistHorizontalCom))
				            (SETQ PP22315 (+ PP22315 DistVerticalCom ))
							(if (and (= (fix PP11315) (fix PP11)) (= (fix PP22315) (fix PP22)))(setq exit315 1))
					)
        )
		   )
	    )	
	)
)
;(print "exit0=")
;(print exit0)
;(print "exit45=")
;(print exit45)
;(print "exit90=")
;(print exit90)
;(print "exit135=")
;(print exit135)
;(print "exit180=")
;(print exit180)
;(print "exit225=")
;(print exit225)
;(print "exit270=")
;(print exit270)
;(print "exit315=")
;(print exit315) 
)