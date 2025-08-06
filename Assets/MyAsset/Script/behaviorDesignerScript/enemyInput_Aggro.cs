using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.AI.Navigation;
using UnityEngine.AI;
using Unity.VisualScripting;
using UnityEngine.UIElements;

namespace enemyInput_Aggro
{
    [TaskCategory("MyAsset")]
    public class enemyCommand_Attack : Action
    {
        //Bボタンを押すだけのコマンド.
        public override void OnAwake()
        {
        }

        //とりあえず指定された位置に近づくだけのスクリプトを組む
        public override void OnStart()
        {
        }
    }
}