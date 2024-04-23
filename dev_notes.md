* To update clients after some state change, there are a few alternatives. I should probably narrow down which ones to use and when, still unsure which is best/when its best
    * Syncvar Hook
        * can only include state changes for single var (in easy mode)
        * a single syncvar should always recieve updates in order, but changing multiple syncvars can lead to race condition
        * hard mode (to avoid race conditions and allow multiple changes in order) : must always change vars in same order AND order declarations within single file
    * ClientRpc/TargetRpc
        * can be called after several server state changes but can lead to race conditions so all state should be included as arg
        * order execution on clients is undetermined    
* Note : when sending structs over network, only public fields are actually sent... watch out for that.
