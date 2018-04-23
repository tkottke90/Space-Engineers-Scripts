#!/usr/local/bin/node
'use strict'

import * as fs from 'fs';
import * as Goals from './lib/Goal.type';

const defaultVals: number[] = [ 0, 1, 0, 0 ];
const thresholds = Goals.thresholds;


class WorldState {
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
}
 

interface Goal {
    normilze(thresholdArr: object ,val: number, val2?: number): number;
    // result(): WorldState;
}

class StayAlive implements Goal {

    normilze(thresholds: Goals.threshold ,dmgLevel:number): number {
        if(dmgLevel > thresholds['StayAlive']){
            return ((dmgLevel-thresholds['StayAlive'])/(1-thresholds['StayAlive']));
        } else { return 0; }
    }


    action(){

    }
}

class StayAwake implements Goal {

    normilze(thresholds: Goals.threshold, pwrAvailable:number): number {
        if(pwrAvailable < thresholds['StayAwake']){
            return Math.abs((((pwrAvailable-thresholds['StayAwakeCrit'])/thresholds['StayAwakeCrit'])*0.5)-0.5);
        } else { return 0; } 
    }
}

class Collect implements Goal {
    
    normilze(threshholds: Goals.threshold, invUsed: number, astLocated: number): number {
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

class AIModule {
    
    MyState: WorldState;
    MyThresholds: object;
    MyGoal: object = {};

    AvailableGoals: Goal[] = [
        new StayAlive(),
        new StayAwake(),
        new Collect()
    ];

    constructor(defaultThresholds: object, state: WorldState){
        this.MyState = state;
        this.MyThresholds = defaultThresholds;
        this.MyGoal;
    }

    run() {

        let goalRel = [
            { name: 'StayAlive', relivancy: this.AvailableGoals[0].normilze(this.MyThresholds, this.MyState.Damage_Taken), goal: this.AvailableGoals[0] },
            { name: 'StayAwake', relivancy: this.AvailableGoals[1].normilze(this.MyThresholds, this.MyState.Power_Avail), goal: this.AvailableGoals[1] },
            { name: 'CollectResources', relivancy: this.AvailableGoals[2].normilze(this.MyThresholds, this.MyState.Inv_Used, this.MyState.Ast_Count), goal: this.AvailableGoals[2] }
        ];

        let selectedGoal: object;
        for (let i = 0; i < goalRel.length; i++){
            if(selectedGoal){
                selectedGoal = selectedGoal['relivancy'] > goalRel[i]['relivancy'] ? selectedGoal : goalRel[i]; 
            } else {
                selectedGoal = goalRel[i];
            }
            
        }
        
        this.MyGoal = selectedGoal;
    }
}

// =====================================
//              Main
// =====================================

let NewWorld = new WorldState(defaultVals);
let AI = new AIModule(thresholds, NewWorld);

let outputData = [];
let outputPath = './data/datafile.json'

for(let i = 0; i < 3; i++){
    AI.run();

    outputData.push({
        'Current_Goal' : AI.MyGoal,
        'States' : AI.MyState
    });
}

fs.writeFile(outputPath, JSON.stringify(outputData),(err) => {
    if(err){ console.log(err) }
    else { console.log(`Data written successfully to: ${outputPath}`); }
});