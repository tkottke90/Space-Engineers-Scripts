export interface threshold {
    StayAlive: number;
    StayAwake: number;
    StayAwakeCrit: number;
    Find: number;
    Find_Weight: number;
    Collect: number;
    Collect_Weight: number;
}

export class RelevancyObject {
    name: string;
    relevancy: number;
    goal: Goal;

    constructor(name: string, rel: number, goal: Goal){
        this.name = name;
        this.relevancy = rel;
        this.goal = goal;
    }


}

const Mutators = {
    Power : {
        Min : 0.01,
        Max : 0.02
    },
    Damage : {
        Min : 0.0,
        Max : 0.1
    },
    Inventory: {
        Min : 0.0,
        Max : 0.1
    }
}

export class WorldState {
    Damage_Taken: number;
    Power_Avail: number;
    Inv_Used: number;
    Ast_Count: number;

    constructor(vals: number[]){
        this.Damage_Taken = vals[0];
        this.Power_Avail = vals[1];
        this.Inv_Used = vals[2];
        this.Ast_Count = vals[3];
    }

    public output(): object{
        return {
            Damage: this.Damage_Taken,
            Power: this.Power_Avail,
            Inventory : this.Inv_Used,
            Asteriod : this.Ast_Count
        }
    }
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

export interface Goal {
    normilze(thresholdArr: object ,val: number, val2?: number): number;
    action(currentState: WorldState): WorldState;
}

export class StayAlive implements Goal {

    underAttack: boolean = false;

    normilze(thresholds: threshold ,dmgLevel:number): number {
        if(dmgLevel > thresholds['StayAlive']){
            return ((dmgLevel-thresholds['StayAlive'])/(1-thresholds['StayAlive']));
        } else { return 0; }
    }


    action(currentState: WorldState): WorldState{
        let newState = currentState;
        newState.Power_Avail -= (Math.random()*(Mutators.Power.Max - Mutators.Power.Min)) + Mutators.Power.Min;
        if((Math.random() * 100) <= (this.underAttack ? 75 : 5)){
            newState.Damage_Taken += (Math.random()*Mutators.Damage.Max)
        }
        return newState;
    }
}

export class StayAwake implements Goal {

    normilze(thresholds: threshold, pwrAvailable:number): number {
        if(pwrAvailable < thresholds['StayAwake']){
            return Math.abs((((pwrAvailable-thresholds['StayAwakeCrit'])/thresholds['StayAwakeCrit'])*0.5)-0.5);
        } else { return 0; } 
    }

    action(currentState: WorldState): WorldState{
        let newState = currentState;
        let random = Math.random();
        newState.Power_Avail -= 1;
        //console.log(`Power_Avail = ${newState.Power_Avail} - ((${random.toFixed()}) * (${Mutators.Power.Max - Mutators.Power.Min}) + ${Mutators.Power.Min}) \n Result: ${newState.Power_Avail}`);
        if((Math.random() * 100) <= 5){
            newState.Damage_Taken += (Math.random()*Mutators.Damage.Max)
        }
        return newState;
    }
}

export class Collect implements Goal {
    
    normilze(threshholds: threshold, invUsed: number, astLocated: number): number {
        let outputVal = 0.0;
        if (astLocated >= thresholds['Find']){
            outputVal += 0.7;
        }

        if(invUsed >= thresholds['Collect']){
            outputVal += (1 - ((invUsed - thresholds['Collect'])/thresholds['Collect'])) * 0.3;
        } else { outputVal += 0.3; }

        return outputVal;
    }

    action(currentState: WorldState): WorldState{
        let newState: WorldState = currentState;
        if(newState.Ast_Count == 0){ // If no known asteriods run Scan action
            // Sim asteriod scan
            newState.Ast_Count = Math.round(Math.random());
            newState.Power_Avail -= 0.1;
        } else if (newState.Inv_Used >= 1) {
            let random = Math.random();
            newState.Power_Avail -= ((random*(Mutators.Power.Max - Mutators.Power.Min)) + Mutators.Power.Min)*2;
            newState.Inv_Used = 0.0;
            newState.Power_Avail = 1.0
        } else {
            let random = Math.random();
            newState.Power_Avail -= (random*(Mutators.Power.Max - Mutators.Power.Min)) + Mutators.Power.Min;
            //console.log(`Power_Avail = ${newState.Power_Avail} - ((${random.toFixed()}) * (${Mutators.Power.Max - Mutators.Power.Min}) + ${Mutators.Power.Min}) \n Result: ${newState.Power_Avail}`);
            newState.Inv_Used += Math.random()*Mutators.Inventory.Max;
            if((Math.random() * 100) <= 5){
                newState.Damage_Taken += (Math.random()*Mutators.Damage.Max)
            }
        }
        return newState;
    }
}