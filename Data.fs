﻿namespace Tasky 
open FSharp.Data.Sql
open System
open System.IO

module Data = 
    [<Literal>]
    let private connectionString = @"Data Source=" + 
                                    __SOURCE_DIRECTORY__ + 
                                    @"/Resources/task.sqlite;Version=3;" 

    type sql = SqlDataProvider<ConnectionString = connectionString,
                               DatabaseVendor = Common.DatabaseProviderTypes.SQLITE,
                               ResolutionPath = // Mono.Data.SqliteClient location
                                   @"/Library/Frameworks/Mono.framework/Libraries/mono/4.5/",
                               UseOptionTypes = false>

    type task = {Id : Int64; Description : string; mutable Complete : bool}

    let private ctx = sql.GetDataContext()

    let GetIncompleteTasks () = 
        query { for data in ctx.``[MAIN].[TASKS]`` do 
                    where (data.COMPLETE = 0L)
                    select {Id = data.ID; Description = data.DESCRIPTION; Complete = false}}
                |> Seq.toList

    let private findTask id =
        ctx.``[MAIN].[TASKS]``
        |> Seq.find (fun t -> t.ID = id)

    let AddTask description = 
        let newTask = ctx.``[MAIN].[TASKS]``.Create()
        newTask.DESCRIPTION <- description
        newTask.COMPLETE <- 0L
        ctx.SubmitUpdates()

    let DeleteTask id = 
        let task = findTask id
        task.Delete()
        ctx.SubmitUpdates()

    let UpdateTask id description complete = 
        let task = findTask id
        task.COMPLETE <- if complete then 1L else 0L
        task.DESCRIPTION <- description
        ctx.SubmitUpdates()
