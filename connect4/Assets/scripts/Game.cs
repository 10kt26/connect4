using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Game : MonoBehaviour
{
    public int MaxSerchDepth=10;
    public SelectTab select_ue;
    public SelectTab select_waku;
    public Coin coin_prefab;
    public WinCircle win_circle_prefab;
    private int[,] state=new int[7,6];//未は0,playerは1,AIは-1
    private int[] piles=new int[7];
    private int phase;
    private int selectColumn;
    private Coin coin;
    // Start is called before the first frame update
    void Start()
    {
        select_ue=Instantiate(select_ue);
        select_waku=Instantiate(select_waku);
        select_ue.visible(false);
        select_waku.visible(false);
        for(int i=0;i<7;i++){
            for(int j=0;j<6;j++){
                state[i,j]=0;
            }
            piles[i]=0;
        }
        phase=1;
        selectColumn=3;
    }

    // Update is called once per frame
    void Update()
    {
        if(phase==1){
            PlayerSelect();
        }
        if(phase==2){
            CoinFall();
        }
        if(phase==3){ 
            if(IsWin(state,selectColumn,piles[selectColumn]-1)) {
                Debug.Log("Player Won!");
                IsWin_draw(state,selectColumn,piles[selectColumn]-1);
                phase=0;
            }
            else if(IsDraw(piles)) {
                Debug.Log("Draw");
                phase=0;
            }else{
                phase=4;
                Debug.Log("AI now serching... Wait for a while");
            }
        }
        if(phase==4){
            AIturn();
            phase=5;
            Debug.Log("AI Selected!");
        }
        if(phase==5){
            CoinFall();
        }
        if(phase==6){
            if(IsWin(state,selectColumn,piles[selectColumn]-1)) {
                Debug.Log("AI Won!");
                IsWin_draw(state,selectColumn,piles[selectColumn]-1);
                phase=0;
            }
            else if(IsDraw(piles)) {
                Debug.Log("Draw");
                phase=0;
            }
            else{
                phase=1;
                selectColumn=3;
                select_ue.transform.position=new Vector3(-7.2f+2.4f*selectColumn,8,0);
                select_waku.transform.position=new Vector3(-7.2f+2.4f*selectColumn,0,-0.405f);
            }
        }
    }

    void PlayerSelect(){
        select_ue.visible(true);
        select_waku.visible(true);
        if(Input.GetKeyDown(KeyCode.RightArrow)&&selectColumn!=6){
            select_ue.transform.position+=new Vector3(2.4f,0,0);
            select_waku.transform.position+=new Vector3(2.4f,0,0);
            selectColumn+=1;
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)&&selectColumn!=0){
            select_ue.transform.position-=new Vector3(2.4f,0,0);
            select_waku.transform.position-=new Vector3(2.4f,0,0);
            selectColumn-=1;
        }
        else if(Input.GetKeyDown(KeyCode.Return)&&piles[selectColumn]!=6){
            state[selectColumn,piles[selectColumn]]=1;//playerは1
            piles[selectColumn]+=1;
            select_ue.visible(false);
            select_waku.visible(false);
            select_ue.transform.position=new Vector3(0,8,0);
            select_waku.transform.position=new Vector3(0,0,-0.405f);
            coin=Instantiate(coin_prefab);
            coin.setColor(1);
            coin.transform.position=new Vector3(-7.2f+selectColumn*2.4f,8,0);
            phase=2;
        }
    }

    void CoinFall(){
        coin.transform.position=Vector3.MoveTowards(coin.transform.position, new Vector3(-7.2f+selectColumn*2.4f,piles[selectColumn]*2-7,0), 10*Time.deltaTime);
        if(coin.transform.position.y<=piles[selectColumn]*2-7){
            if(phase==2){
                phase=3;
            }
            if(phase==5){
                phase=6;
            }
        }
    }

    void print_state(int[,] state_printed){
        for(int j=5;j>=0;j--){
            string str="state_printed["+j.ToString()+"]: ";
            for(int i=0;i<7;i++){
                str=str+state_printed[i,j].ToString()+" ";
            }
            Debug.Log(str);
        }
        Debug.Log("----------------------------------------");
    }

    void AIturn(){
        float[] scores=new float[7];
        for(int i=0;i<7;i++) scores[i]=-100f;
        float alpha=-100f;
        int selected_i=0;
        int[] ordered7 = new int[] { 0, 1, 2, 3, 4, 5, 6};
        System.Random random = new System.Random();	
		int[] random7 = ordered7.OrderBy(x => random.Next()).ToArray();
        for(int k=0;k<7;k++){
            int i=random7[k];
            if(piles[i]!=6){
                int[,] copied_state=new int[7,6];
                int[] copied_piles=new int[7];
                Array.Copy(state, copied_state, state.Length);
                Array.Copy(piles, copied_piles, piles.Length);
                copied_state[i,copied_piles[i]]=-1;
                int[,] tmp42=new int[4,2]; 
                if(IsWin(copied_state,i,copied_piles[i])){
                    scores[i]=1f;
                    selected_i=i;
                    break;
                }
                copied_piles[i]+=1;
                if(IsDraw(copied_piles)){
                    scores[i]=0f;
                    selected_i=i;
                    break;
                }
                int PlayerOrAI=-1;
                scores[i]=-alphabeta(copied_state,copied_piles,PlayerOrAI,-100f,-alpha,MaxSerchDepth);
                if(scores[i]>alpha){
                    selected_i=i;
                    alpha=scores[i];
                }
            }
        }
        //for(int i=0;i<7;i++) Debug.Log("scores["+random7[i].ToString()+"] "+scores[random7[i]].ToString());
        selectColumn=selected_i;
        state[selectColumn,piles[selectColumn]]=-1;//AIは-1
        piles[selectColumn]+=1;
        coin=Instantiate(coin_prefab);
        coin.setColor(-1);
        coin.transform.position=new Vector3(-7.2f+selectColumn*2.4f,8,0);
    }

    float alphabeta(int[,] copied_state,int[] copied_piles,int input_PlayerOrAI,float alpha,float beta,int depth){
        if(depth==0) return 0f;
        float score;
        float waribiki=0.9f;
        int PlayerOrAI=-input_PlayerOrAI;
        for(int i=0;i<7;i++){
            if(copied_piles[i]!=6){
                int[,] copied_copied_state=new int[7,6];
                int[] copied_copied_piles=new int[7];
                Array.Copy(copied_state, copied_copied_state, copied_state.Length);
                Array.Copy(copied_piles, copied_copied_piles, copied_piles.Length);
                copied_copied_state[i,copied_copied_piles[i]]=PlayerOrAI;
                if(IsWin(copied_copied_state,i,copied_copied_piles[i])){
                    score=1f;
                    return score;
                }
                copied_copied_piles[i]+=1;
                if(IsDraw(copied_copied_piles)){
                    score=0f;
                    return score;
                }
                score=-waribiki*alphabeta(copied_copied_state,copied_copied_piles,PlayerOrAI,-beta,-alpha,depth-1);
                if(score>=beta){
                    return score;
                }
                if(score>alpha){
                   alpha=score;
                }
            }
        }
        return alpha;        
    }

    void DrawCircle(int[,] input42){
        for(int i=0;i<4;i++){
            WinCircle win_circle=Instantiate(win_circle_prefab);
            win_circle.transform.Rotate(90,0,0);
            win_circle.transform.position=new Vector3(-7.2f+2.4f*input42[i,0],-5+2*input42[i,1],-0.405f);
        }
    }

    void init42(int[,] inited42){
        for(int i=0;i<4;i++){
            for(int j=0;j<2;j++){
                inited42[i,j]=0;
            }
        }
    }

    bool IsWin(int[,] stateWin,int cur_w,int cur_h){
        int counter=0;
        //下
        for(int i=cur_h-1;i>=0;i--){
            if(stateWin[cur_w,cur_h]==stateWin[cur_w,i]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            } 
        }

        counter=0;
        //横
            //まず右
        for(int i=cur_w+1;i<7;i++){
            if(stateWin[cur_w,cur_h]==stateWin[i,cur_h]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            }
        }
            //次に左
        for(int i=cur_w-1;i>=0;i--){
            if(stateWin[cur_w,cur_h]==stateWin[i,cur_h]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            }
        }

        counter=0;
        //右上ー左下
            //まず右上
        for(int i=1;i<4;i++){
            if(cur_w+i==7||cur_h+i==6){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w+i,cur_h+i]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            }
        }
            //次に左下
        for(int i=1;i<4;i++){
            if(cur_w-i<0||cur_h-i<0){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w-i,cur_h-i]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            }
        }

        counter=0;
        //右下ー左上
            //まず右下
        for(int i=1;i<4;i++){
            if(cur_w+i==7||cur_h-i<0){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w+i,cur_h-i]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            }
        }
            //次に左上
        for(int i=1;i<4;i++){
            if(cur_w-i<0||cur_h+i==6){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w-i,cur_h+i]){
                counter+=1;
                if(counter==3){
                    return true;
                }
            }else{
                break;
            }
        }

        return false;
    }
    
    bool IsWin_draw(int[,] stateWin,int cur_w,int cur_h){
        int[,] buffer=new int[4,2];

        init42(buffer);
        buffer[0,0]=cur_w;
        buffer[0,1]=cur_h;
        int counter=0;
        //下
        for(int i=cur_h-1;i>=0;i--){
            if(stateWin[cur_w,cur_h]==stateWin[cur_w,i]){
                counter+=1;
                buffer[counter,0]=cur_w;
                buffer[counter,1]=i;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            } 
        }

        init42(buffer);
        buffer[0,0]=cur_w;
        buffer[0,1]=cur_h;
        counter=0;
        //横
            //まず右
        for(int i=cur_w+1;i<7;i++){
            if(stateWin[cur_w,cur_h]==stateWin[i,cur_h]){
                counter+=1;
                buffer[counter,0]=i;
                buffer[counter,1]=cur_h;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            }
        }
            //次に左
        for(int i=cur_w-1;i>=0;i--){
            if(stateWin[cur_w,cur_h]==stateWin[i,cur_h]){
                counter+=1;
                buffer[counter,0]=i;
                buffer[counter,1]=cur_h;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            }
        }

        init42(buffer);
        buffer[0,0]=cur_w;
        buffer[0,1]=cur_h;
        counter=0;
        //右上ー左下
            //まず右上
        for(int i=1;i<4;i++){
            if(cur_w+i==7||cur_h+i==6){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w+i,cur_h+i]){
                counter+=1;
                buffer[counter,0]=cur_w+i;
                buffer[counter,1]=cur_h+i;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            }
        }
            //次に左下
        for(int i=1;i<4;i++){
            if(cur_w-i<0||cur_h-i<0){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w-i,cur_h-i]){
                counter+=1;
                buffer[counter,0]=cur_w-i;
                buffer[counter,1]=cur_h-i;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            }
        }

        init42(buffer);
        buffer[0,0]=cur_w;
        buffer[0,1]=cur_h;
        counter=0;
        //右下ー左上
            //まず右下
        for(int i=1;i<4;i++){
            if(cur_w+i==7||cur_h-i<0){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w+i,cur_h-i]){
                counter+=1;
                buffer[counter,0]=cur_w+i;
                buffer[counter,1]=cur_h-i;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            }
        }
            //次に左上
        for(int i=1;i<4;i++){
            if(cur_w-i<0||cur_h+i==6){
                break;
            }
            if(stateWin[cur_w,cur_h]==stateWin[cur_w-i,cur_h+i]){
                counter+=1;
                buffer[counter,0]=cur_w-i;
                buffer[counter,1]=cur_h+i;
                if(counter==3){
                    DrawCircle(buffer);
                    return true;
                }
            }else{
                break;
            }
        }

        return false;
    }
    
    bool IsDraw(int[] pilesDraw){
        for(int i=0;i<7;i++){
            if(pilesDraw[i]!=6) return false;
        }
        return true;
    }
}
