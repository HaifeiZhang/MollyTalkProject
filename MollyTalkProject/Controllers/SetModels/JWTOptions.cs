﻿namespace MollyTalkProject.Controllers.SetModels
{
    public class JWTOptions
    {
        public string SigningKey { get; set; }
        public int ExpireSeconds { get; set; }
    }
}
