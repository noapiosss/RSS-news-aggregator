using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services
{
    public class TokenHandler : ITokenHandler
    {
        public bool Validate(string token, out string username)
        {
            JwtSecurityTokenHandler tokenHandler = new();
            SymmetricSecurityKey key = new(System.Text.Encoding.UTF8.GetBytes("Smiley face with small eyes"));

            try
            {
                _ = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                username = jwtToken.Claims.First().Value;
                return true;
            }
            catch
            {
                username = null;
                return false;
            }
        }

        public string GenerateToken(string username)
        {
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, username)
            };

            SymmetricSecurityKey key = new(System.Text.Encoding.UTF8.GetBytes("Smiley face with small eyes"));
            SigningCredentials cred = new(key, SecurityAlgorithms.HmacSha256Signature);
            JwtSecurityToken token = new(
                claims: claims,
                expires: DateTime.Now.AddMonths(1),
                signingCredentials: cred);

            string jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}