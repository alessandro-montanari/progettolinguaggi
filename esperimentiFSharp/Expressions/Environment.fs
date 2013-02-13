module Environment

open System.Collections.Generic

// Mi serve un'Environment "doppio" per tenere i valori scalari e le liste di valori
type Environment() =
    
    let _envSingle = new Dictionary<string, double>(HashIdentity.Structural)
    let _envSeries = new Dictionary<string, double list>(HashIdentity.Structural)

    member this.EnvSingle = _envSingle
    member this.EnvSeries = _envSeries
