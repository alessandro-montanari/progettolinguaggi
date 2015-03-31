**Inserire funzioni built-in direttamente nella grammatica**
  * Vantaggi
    * Durante il riconoscimento si individuano errori nell'utilizzo dei built-in, ad esempio se scrivo "sinm", altrimenti questi errori vengono individuati a tempo di valutazione.
  * Svantaggi
    * se devo aggiungere nuovi built-in devo andare a modificare la grammatica e probabilmente l'AST (sicuramente devo modificare gli interpreti). Se non includo i built-in nella grammatica e ne aggiungo di nuovi, non devo modificare la grammatica ma solo gli interpreti (e forse l'AST).