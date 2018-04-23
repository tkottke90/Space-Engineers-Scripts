export interface threshold {
    StayAlive: number;
    StayAwake: number;
    StayAwakeCrit: number;
    Find: number;
    Find_Weight: number;
    Collect: number;
    Collect_Weight: number;
}

export let thresholds = {
    'StayAlive' : 0.25,
    'StayAwake' : 0.35,
    'StayAwakeCrit' : 0.175,
    'Find': 1,
    'Find_Weight': 0.7,
    'Collect': 0.5,
    'Collect_Weight': 0.3
}

interface Goal {
    normilze(val: number, val2?: number): number;
    // result(): WorldState;
}

export class StayAlive implements Goal {

    normilze(dmgLevel:number): number {
        if(dmgLevel > thresholds['StayAlive']){
            return ((dmgLevel-thresholds['StayAlive'])/(1-thresholds['StayAlive']));
        } else { return 0; }
    }


    action(){

    }
}

export class StayAwake implements Goal {

    normilze(pwrAvailable:number): number {
        if(pwrAvailable < thresholds['StayAwake']){
            return Math.abs((((pwrAvailable-thresholds['StayAwakeCrit'])/thresholds['StayAwakeCrit'])*0.5)-0.5);
        } else { return 0; } 
    }
}

export class Collect implements Goal {
    
    normilze(invUsed: number, astLocated: number): number {
        let outputVal = 0.0;
        if (astLocated >= thresholds['Find']){
            outputVal += 0.7;
        }

        if(invUsed >= thresholds['Collect']){
            outputVal += (1 - ((invUsed - thresholds['Collect'])/thresholds['Collect'])) * 0.3;
        } else { outputVal += 0.3; }

        return outputVal;
    }
}