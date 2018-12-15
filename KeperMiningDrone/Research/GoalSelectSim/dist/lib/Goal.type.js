"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
class RelevancyObject {
    constructor(name, rel, goal) {
        this.name = name;
        this.relevancy = rel;
        this.goal = goal;
    }
}
exports.RelevancyObject = RelevancyObject;
const Mutators = {
    Power: {
        Min: 0.01,
        Max: 0.02
    },
    Damage: {
        Min: 0.0,
        Max: 0.1
    },
    Inventory: {
        Min: 0.0,
        Max: 0.1
    }
};
class WorldState {
    constructor(vals) {
        this.Damage_Taken = vals[0];
        this.Power_Avail = vals[1];
        this.Inv_Used = vals[2];
        this.Ast_Count = vals[3];
    }
    output() {
        return {
            Damage: this.Damage_Taken,
            Power: this.Power_Avail,
            Inventory: this.Inv_Used,
            Asteriod: this.Ast_Count
        };
    }
}
exports.WorldState = WorldState;
exports.thresholds = {
    'StayAlive': 0.25,
    'StayAwake': 0.35,
    'StayAwakeCrit': 0.175,
    'Find': 1,
    'Find_Weight': 0.7,
    'Collect': 0.5,
    'Collect_Weight': 0.3
};
class StayAlive {
    constructor() {
        this.underAttack = false;
    }
    normilze(thresholds, dmgLevel) {
        if (dmgLevel > thresholds['StayAlive']) {
            return ((dmgLevel - thresholds['StayAlive']) / (1 - thresholds['StayAlive']));
        }
        else {
            return 0;
        }
    }
    action(currentState) {
        let newState = currentState;
        newState.Power_Avail -= (Math.random() * (Mutators.Power.Max - Mutators.Power.Min)) + Mutators.Power.Min;
        if ((Math.random() * 100) <= (this.underAttack ? 75 : 5)) {
            newState.Damage_Taken += (Math.random() * Mutators.Damage.Max);
        }
        return newState;
    }
}
exports.StayAlive = StayAlive;
class StayAwake {
    normilze(thresholds, pwrAvailable) {
        if (pwrAvailable < thresholds['StayAwake']) {
            return Math.abs((((pwrAvailable - thresholds['StayAwakeCrit']) / thresholds['StayAwakeCrit']) * 0.5) - 0.5);
        }
        else {
            return 0;
        }
    }
    action(currentState) {
        let newState = currentState;
        let random = Math.random();
        newState.Power_Avail -= 1;
        //console.log(`Power_Avail = ${newState.Power_Avail} - ((${random.toFixed()}) * (${Mutators.Power.Max - Mutators.Power.Min}) + ${Mutators.Power.Min}) \n Result: ${newState.Power_Avail}`);
        if ((Math.random() * 100) <= 5) {
            newState.Damage_Taken += (Math.random() * Mutators.Damage.Max);
        }
        return newState;
    }
}
exports.StayAwake = StayAwake;
class Collect {
    normilze(threshholds, invUsed, astLocated) {
        let outputVal = 0.0;
        if (astLocated >= exports.thresholds['Find']) {
            outputVal += 0.7;
        }
        if (invUsed >= exports.thresholds['Collect']) {
            outputVal += (1 - ((invUsed - exports.thresholds['Collect']) / exports.thresholds['Collect'])) * 0.3;
        }
        else {
            outputVal += 0.3;
        }
        return outputVal;
    }
    action(currentState) {
        let newState = currentState;
        if (newState.Ast_Count == 0) {
            // Sim asteriod scan
            newState.Ast_Count = Math.round(Math.random());
            newState.Power_Avail -= 0.1;
        }
        else if (newState.Inv_Used >= 1) {
            let random = Math.random();
            newState.Power_Avail -= ((random * (Mutators.Power.Max - Mutators.Power.Min)) + Mutators.Power.Min) * 2;
            newState.Inv_Used = 0.0;
            newState.Power_Avail = 1.0;
        }
        else {
            let random = Math.random();
            newState.Power_Avail -= (random * (Mutators.Power.Max - Mutators.Power.Min)) + Mutators.Power.Min;
            //console.log(`Power_Avail = ${newState.Power_Avail} - ((${random.toFixed()}) * (${Mutators.Power.Max - Mutators.Power.Min}) + ${Mutators.Power.Min}) \n Result: ${newState.Power_Avail}`);
            newState.Inv_Used += Math.random() * Mutators.Inventory.Max;
            if ((Math.random() * 100) <= 5) {
                newState.Damage_Taken += (Math.random() * Mutators.Damage.Max);
            }
        }
        return newState;
    }
}
exports.Collect = Collect;
