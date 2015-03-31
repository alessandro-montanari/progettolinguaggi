

# Introduzione #
Implementazione rete neurale + DSL per descrivere la rete e per la sua simulazione

# Caratteristiche Linguaggio #
Per costruire una rete neurale occorre definire in generale:
  * il modello dei neuroni (ingressi e loro tipo, funzione di attivazione e funzione di uscita);
  * l'architettura della rete (numero di neuroni, completamente connessa, a layer);
  * la modalità di attivazione dei neuroni (sincrona o asincrona);
  * paradigma di apprendimento (supervisionato, competitivo o con rinforzo);

Concettualmente il linguaggio è diviso in tre parti:
  1. una per la definizione della rete vera e propria
  1. una per la fase di preprocessing dei dati
  1. una per la fase di post-processing cioè per definire come deve avvenire la validazione della rete


## Formato dati in ingresso ##
Per semplicità e per compatibilità con Weka si è deciso di adottare il formato ARFF di quest'ultimo. Non si è ritenuto necessario apportare modifiche a questo formato.

Solo tre tipi di dato su quattro sono supportati, in particolare: string, number e nominal.

An ARFF file consists of two distinct sections:
  * the Header section defines attribute name, type and relations.
```
@Relation <data-name>
@attribute <attribute-name> <type> or {range}
```

  * the Data section lists the data records.
```
@Data
list of data instances
```

  * Any line start with % is the comments.

## Preprocessing ##
Si è deciso di rendere disponibili solo filtri di tipo **unsupervised** ma applicabili sia agli **attributi** che alle **istanze**.

Filtri sugli attributi:
  * **AddExpression**
    * crea un nuovo attributo applicando una funzione matematica agli altri attributi.
    * **name**: nome attributo
    * **expression**: espressione vera e propria (+, -, `*`, /, ^, log, ..., MEAN, MAX, MIN, SD, SUM, SUMSQUARED) (stringa)
  * **Discretize**
    * converte attributi numeric in nominal
    * **indices**: attributi ai quali applicare il filtro. Lista separata da , oppure range con ".." per definire un range con estremi inclusi.
    * **bins**: numero di intervalli in cui deve essere diviso l'attributo
  * **MathExpression**
    * simile a AddExpression ma modifica il valore dell'attributo invece di aggiungerne uno nuovo. Praticamente viene calcolata l'espressione fornita per ogni attributo specificato in indices e il nuovo valore viene sostituito al vecchio. E' possibile fare riferimento all'attributo corrente con l'identificatore "A", denotando il valore "vecchio".
    * Questo filtro è stato pensato per modificare il valore di uno o più attributi tramite un'espressione che coinvolga solo quell'attributo (es. A + 100), quindi se fa riferimento ad altri attributi ci sono le seguenti limitazioni: l'espressione fornita viene valutata per tutti gli attributi presenti in "attributes" nello stesso ordine in cui gli attributi sono specificati, quindi se nell'espressione si fa riferimento ad attributi specificati in "attributes", in base alla loro posizione nella lista, potrebbe essere utilizzato o il valore vecchio (prima dell'applicazione della funzione) o quello nuovo. Se si utilizza un attributo all'interno di una aggregate function (min, max, ...) quell'attributo denota sempre il vecchio range di valori (prima dell'applicazione della espressione).
    * **attributes**: attributi ai quali applicare il filtro (come sopra).
    * **expression**: espressione vera e propria (come sopra)
  * **NominalToBinary**
    * converte gli attributi nominali in attributi numerici binari (cono solo due valori, 0 e 1). Un attributo con k valori viene trasformato in k attributi binari. Gli attributi numeric vengono lasciati numeric.
    * I nuovi attributi nominali hanno il nome con il seguente formato: `<nome att>=<valore>`
    * **indices**: attributi ai quali applicare il filtro (come sopra).
  * **Normalize**
    * Scala tutti i valori numeric del data set per farli rientrare nel range [0, 1]
    * xnew = ((x - xmin) / (xmax - xmin) `*` scale) + translation
    * **scale**: Il fattore di scala (default=1)
    * **translation**: il fattore di traslazione dell'output (default=0)
  * **RemoveByName**
    * rimuove attributi
    * **indices**: attributi ai quali applicare il filtro (come sopra).
  * **RemoveType**
    * rimuove attributi di un certo tipo
    * **type**: tipo da rimuovere ("string", "numeric" o "nominal")
  * **ReplaceMissingValues**
    * sostituisce tutti i valori mancanti degli attributi nominal e numeric rispettivamente con la moda e con la media ricavate dal data set per ogni attributo
  * **Standardize**
    * standardizza tutti gli attributi numerici in modo che abbiano media=0 e varianza=1
    * xnew = (x - mu) / sigma

Filtri sulle istanze:
Questi filtri sono applicati a tutte le istanze del data set invece che solo a particolari attributi.
  * **RemovePercentage**
    * rimuove una certa percentuale del data set
    * **percentage**
  * **RemoveRange**
    * rimuove un certo range di istanze dal data set
    * **indices**: istanze alle quali applicare il filtro (come sopra).
  * **SubsetByExpression**
    * rimuove le istanze che ritornano true ad una espressione logica fornita
    * **expression**: espressione logica. E' possibile utilizzare costanti e fare riferimento agli attributi

**Dettaglio sulle espressioni aritmetiche**
  * Operatori classici: +, - `*`, /, ^
  * Uso di parentesi tonde
  * Funzioni trigonometriche: sen(), cos(), tan(), atan(), asen(), acos(), ...
  * Funzioni logaritmiche: log(), ln()
  * Altre funzioni per singoli valori: floor(), ceil(), sqrt()
  * Altre funzioni per serie di valori: mean(), sd(), min(), max(), sum(), sumsquared(), sumOfProductes(x;y). In questi casi è possibile fornire una lista di valori nel formato `[ x1, x2, x4 ]`, oppure specificare un attributo (come definito sotto) e verrà quindi calcolata la funzione per tutti i valori di quell'attributo.

**Dettaglio sulle espressioni logiche**
  * sintassi del C (valori double come boolean: 1.0 -> true, 0.0 -> false)

**NB**: ogni volta che è possibile fare riferimento ad un attributo è possibile utilizzare una delle seguenti notazioni:
  * Nome dell'attributo così come compare nel file ARFF racchiuso tra doppi
  * `ATT<index>`, dove index è l'indice dell'attributo in base 0

**NB2**: ogni volta che è possibile fare riferimento ad una istanza del data set è possibile utilizzare la seguente notazione: `INST<index>`, dove index è l'indice dell'istanza in base 0

**NB3**: ogni volta in cui è possibile specificare una lista di attributi o una lista di istanze, è possibile invertire il significato della lista anteponendo il carattere "!" alla lista, ad esempio se [ATT1, ATT3 .. ATT7] indica la lista di attributi [ATT1, ATT2, ATT3, ATT4, ATT5, ATT6, ATT7], la lista ![ATT1, ATT3 .. ATT7] indica la lista [ATT0, ATT2, ATT8, ATT9, ATT10] (assumendo che siano presenti 10 attributi).

**Sviluppi futuri**
  * Possibilità di fornire le proprie routine di preprocessing e di salvarle/caricarle
  * Possibilità di visualizzare i dati dopo il preprocessing ma prima del training
  * preprocessing dei dati interattivo (applico i filtri e vedo subito il risultato)
  * Possibilità di definire variabili e costanti
  * Altri filtri
    * **NumericTransform**
      * trasforma gli attributi numerici utilizzando la funzione F# fornita
      * **indices**: attributi ai quali applicare il filtro (come sopra).
      * **function**: funzione F# (sintassi completa) da applicare all'attributo (double -> double).

## Rete Neurale ##
Ci concentriamo sulle reti stratificate con apprendimento supervisionato.

Le seguenti considerazioni sono state fatte per la progettazione (tenendo come obbiettivo le reti con apprendimento supervisionato):
  * tipologie di reti diverse hanno descrizioni diverse
  * tipologie di reti diverse hanno algoritmi di training diversi
  * algoritmi di training diversi hanno parametri diversi
  * data una stessa tipologia di rete sono di solito disponibili algoritmi di training diversi

Viste le considerazioni si è deciso di sviluppare il sistema e il linguaggio in modo tale che entrambi siano sufficientemente generali per poter descrivere diverse tipologie di reti e algoritmi e allo stesso tempo sia facile aggiungere nuove implementazioni per reti e algoritmi. In particolare, per questo progetto è stata sviluppata una rete Multi Layer completamente connessa (tra un livello e l'altro), senza shortcut e circuiti ed l'algoritmo classico di Back Propagation.

Le implementazioni delle reti e degli algoritmi possono essere fornite sotto forma di classi .NET (scritte in uno dei qualsiasi linguaggi disponibili) e possono essere caricate nel sistema per poterle utilizzare nel linguaggio.

**Note sulla implementazione**
  * Come trattare l'uscita se si sceglie di classificare secondo un attributo che è numerico ?
    * In questo caso la funzione di uscita deve essere lineare (y = x), altrimenti, se si scegliesse sigmoid, l'uscita non potrebbe mai oltrepassare y = 1. Comunque questo è un "problema" dell'utente che sa quale funzione utilizzare in ogni caso.
    * http://weka.8497.n7.nabble.com/basic-help-in-weka-neural-nets-td16245.html
    * http://www.autonlab.org/tutorials/neural13.pdf
    * http://heuristically.wordpress.com/2011/11/17/using-neural-network-for-regression/
    * http://msdn.microsoft.com/en-us/library/dd206980.aspx
    * http://www.youtube.com/watch?v=m7kpIBGEdkI
  * Come si gestiscono gli attributi stringa?
    * di solito vengono eliminati perché è difficile ricavare informazioni utili da porre in ingresso ad una rete neurale.
  * Come gestire gli attributi nominali in ingresso alla rete?
    * si utilizzano gli indici dei singoli valori come ingressi. L'indice deve essere in base 1 (1, 2, 3, ..) perché lo zero è utilizzato per i valori mancanti (missing).
    * ttps://list.scms.waikato.ac.nz/pipermail/wekalist/2012-February/054790.html
  * Come scelgo la predizione su un un attributo nominale dato che l'uscita dei neuroni è reale?
    * Essendoci un neurone per ogni valore nominale di uscita, la predizione viene scelta come il valore associato al neurone che ha l'uscita massima.
  * Come gestire le istanze con attributi mancanti (missing values)?
    * l'ingresso per quell'attributo viene messo a zero (ignorato).
    * https://list.scms.waikato.ac.nz/pipermail/wekalist/2007-April/036294.html
    * http://forums.pentaho.com/showthread.php?96337-Missing-Values-in-Multilayer-Perceptron-(MLP-Neural-Networks)

**Sviluppi futuri**
  * Connessioni shortcut
  * Recurrent Neural Network (http://en.wikipedia.org/wiki/Recurrent_neural_network)
  * modificare la rete a training time (tipo shell interattiva)
  * possibilità di fare degli snapshot della rete in momenti diversi
  * salvare/ripristinare la rete per non dover rifare il training ogni volta
  * Apprendimento con rinforzo
  * Apprendimento competitivo (Selft Organizing Maps)

## Post Processing e valutazione ##
**Opzioni di test**
  * usare lo stesso training set
  * fornire un diverso test set
  * usare una parte del training set (percentuale) per il test

**Statistiche per predizione su attributi nominali**
  * numero di istanze classificate bene e male
  * percentuale di istanze classificate bene e male

**Statistiche per predizione su attributi numerici**
  * **Mean Absolute Error**
> > ![http://upload.wikimedia.org/math/8/9/f/89f3f847beb7a4c84e31f73a8457c575.png](http://upload.wikimedia.org/math/8/9/f/89f3f847beb7a4c84e31f73a8457c575.png)


> The MAE measures the average magnitude of the errors in a set of forecasts, without considering their direction. It measures accuracy for continuous variables. Expressed in words, the MAE is the average over the verification sample of the absolute values of the differences between forecast and the corresponding observation. The MAE is a linear score which means that all the individual differences are weighted equally in the average.
  * **Mean Squared Error**
> > ![http://upload.wikimedia.org/math/5/0/a/50a70f81235c2f0ed4e6341e39181ae0.png](http://upload.wikimedia.org/math/5/0/a/50a70f81235c2f0ed4e6341e39181ae0.png)


> An MSE of zero, meaning that the estimator  predicts observations of the parameter  with perfect accuracy, is the ideal, but is practically never possible.
  * **Root Mean Squared Error**
> > ![http://upload.wikimedia.org/math/a/4/7/a47161af0a89ce6059aafd43c524839d.png](http://upload.wikimedia.org/math/a/4/7/a47161af0a89ce6059aafd43c524839d.png)


> The RMSE is a quadratic scoring rule which measures the average magnitude of the error Expressing the formula in words, the difference between forecast and corresponding observed values are each squared and then averaged over the sample. Finally, the square root of the average is taken. Since the errors are squared before they are averaged, the RMSE gives a relatively high weight to large errors.         This means the RMSE is most useful when large errors are particularly undesirable.
> RMSE is a good measure of accuracy, but only to compare forecasting errors of different models for a particular variable and not between variables.

The MAE and the RMSE can be used together to diagnose the variation in the errors in a set of forecasts. The RMSE will always be larger or equal to the MAE; the greater difference between them, the greater the variance in the individual errors in the sample. If the RMSE=MAE, then all the errors are of the same magnitude

Both the MAE and RMSE can range from 0 to ∞. They are negatively-oriented scores: Lower values are better.

**Sviluppi futuri**
  * Decidere quali statistiche devono essere calcolate