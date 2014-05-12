﻿//
// BodyPart.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013-2014 Xamarin Inc. (www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Text;
using System.Collections.Generic;

using MimeKit;
using MimeKit.Utils;

namespace MailKit {
	/// <summary>
	/// An abstract body part of a message.
	/// </summary>
	/// <remarks>
	/// Each body part will actually be a <see cref="BodyPartBasic"/>,
	/// <see cref="BodyPartText"/>, <see cref="BodyPartMessage"/>, or
	/// <see cref="BodyPartMultipart"/>.
	/// </remarks>
	public abstract class BodyPart
	{
		/// <summary>
		/// Gets the Content-Type of the body part.
		/// </summary>
		/// <value>The content type.</value>
		public ContentType ContentType {
			get; internal set;
		}

		/// <summary>
		/// Gets the part specifier.
		/// </summary>
		/// <value>The part specifier.</value>
		public string PartSpecifier {
			get; internal set;
		}

		internal protected static void Encode (StringBuilder builder, uint value)
		{
			builder.Append (value.ToString ());
		}

		internal protected static void Encode (StringBuilder builder, string value)
		{
			if (value != null)
				builder.Append (MimeUtils.Quote (value));
			else
				builder.Append ("NIL");
		}

		internal protected static void Encode (StringBuilder builder, string[] values)
		{
			if (values == null || values.Length == 0) {
				builder.Append ("NIL");
				return;
			}

			builder.Append ('(');

			for (int i = 0; i < values.Length; i++) {
				if (i > 0)
					builder.Append (' ');

				Encode (builder, values[i]);
			}

			builder.Append (')');
		}

		internal protected static void Encode (StringBuilder builder, ParameterList parameters)
		{
			if (parameters == null || parameters.Count == 0) {
				builder.Append ("NIL");
				return;
			}

			builder.Append ('(');

			for (int i = 0; i < parameters.Count; i++) {
				if (i > 0)
					builder.Append (' ');

				builder.Append ('(');
				Encode (builder, parameters[i].Name);
				builder.Append (' ');
				Encode (builder, parameters[i].Value);
				builder.Append (')');
			}

			builder.Append (')');
		}

		internal protected static void Encode (StringBuilder builder, ContentDisposition disposition)
		{
			if (disposition == null) {
				builder.Append ("NIL");
				return;
			}

			builder.Append ('(');
			Encode (builder, disposition.Disposition);
			builder.Append (' ');
			Encode (builder, disposition.Parameters);
			builder.Append (')');
		}

		internal protected static void Encode (StringBuilder builder, ContentType contentType)
		{
			Encode (builder, contentType.MediaType);
			builder.Append (' ');
			Encode (builder, contentType.MediaSubtype);
			builder.Append (' ');
			Encode (builder, contentType.Parameters);
		}

		internal protected static void Encode (StringBuilder builder, BodyPartCollection parts)
		{
			if (parts == null || parts.Count == 0) {
				builder.Append ("NIL");
				return;
			}

			builder.Append ('(');

			for (int i = 0; i < parts.Count; i++) {
				if (i > 0)
					builder.Append (' ');

				Encode (builder, parts[i]);
			}

			builder.Append (')');
		}

		internal protected static void Encode (StringBuilder builder, Envelope envelope)
		{
			if (envelope == null) {
				builder.Append ("NIL");
				return;
			}

			envelope.Encode (builder);
		}

		internal protected static void Encode (StringBuilder builder, BodyPart body)
		{
			if (body == null) {
				builder.Append ("NIL");
				return;
			}

			builder.Append ('(');
			body.Encode (builder);
			builder.Append (')');
		}

		/// <summary>
		/// Encodes the <see cref="BodyPart"/> into the <see cref="System.Text.StringBuilder"/>.
		/// </summary>
		/// <remarks>
		/// Encodes the <see cref="BodyPart"/> into the <see cref="System.Text.StringBuilder"/>.
		/// </remarks>
		/// <param name="builder">The string builder.</param>
		protected abstract void Encode (StringBuilder builder);

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MailKit.BodyPart"/>.
		/// </summary>
		/// <remarks>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="MailKit.BodyPart"/>.
		/// </remarks>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="MailKit.BodyPart"/>.</returns>
		public override string ToString ()
		{
			var builder = new StringBuilder ();

			builder.Append ('(');
			Encode (builder);
			builder.Append (')');

			return builder.ToString ();
		}

		static bool TryParse (string text, ref int index, out uint value)
		{
			while (index < text.Length && text[index] == ' ')
				index++;

			int startIndex = index;

			value = 0;

			while (index < text.Length && char.IsDigit (text[index]))
				value = (value * 10) + (uint) (text[index++] - '0');

			return index > startIndex;
		}

		static bool TryParse (string text, ref int index, out string nstring)
		{
			nstring = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length)
				return false;

			if (text[index] == '"') {
				var token = new StringBuilder ();
				bool escaped = false;

				index++;

				while (index < text.Length) {
					if (text[index] == '"' && !escaped)
						break;

					if (escaped || text[index] != '\\') {
						token.Append (text[index]);
					} else {
						escaped = true;
					}

					index++;
				}

				if (index >= text.Length)
					return false;

				nstring = token.ToString ();

				index++;
			} else {
				int startIndex = index;

				while (index < text.Length && text[index] != ' ')
					index++;

				nstring = text.Substring (startIndex, index - startIndex);

				if (nstring == "NIL")
					nstring = null;
			}

			return true;
		}

		static bool TryParse (string text, ref int index, out string[] values)
		{
			values = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length)
				return false;

			if (text[index] != '(') {
				if (index + 3 <= text.Length && text.Substring (index, 3) == "NIL") {
					index += 3;
					return true;
				}

				return false;
			}

			index++;

			if (index >= text.Length)
				return false;

			var list = new List<string> ();
			string value;

			do {
				if (text[index] == ')')
					break;

				if (!TryParse (text, ref index, out value))
					return false;

				list.Add (value);
			} while (index < text.Length);

			if (index >= text.Length || text[index] != ')')
				return false;

			index++;

			return true;
		}

		static bool TryParse (string text, ref int index, out IList<Parameter> parameters)
		{
			string name, value;

			parameters = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length)
				return false;

			if (text[index] != '(') {
				if (index + 3 <= text.Length && text.Substring (index, 3) == "NIL") {
					index += 3;
					return true;
				}

				return false;
			}

			index++;

			if (index >= text.Length)
				return false;

			parameters = new List<Parameter> ();

			do {
				if (text[index] == ')')
					break;

				if (text[index] != '(')
					return false;

				index++;

				if (!TryParse (text, ref index, out name))
					return false;

				if (!TryParse (text, ref index, out value))
					return false;

				if (index >= text.Length || text[index] != ')')
					return false;

				parameters.Add (new Parameter (name, value));

				index++;
			} while (index < text.Length);

			if (index >= text.Length || text[index] != ')')
				return false;

			index++;

			return true;
		}

		static bool TryParse (string text, ref int index, out ContentDisposition disposition)
		{
			IList<Parameter> parameters;
			string value;

			disposition = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length)
				return false;

			if (text[index] != '(') {
				if (index + 3 <= text.Length && text.Substring (index, 3) == "NIL") {
					index += 3;
					return true;
				}

				return false;
			}

			index++;

			if (!TryParse (text, ref index, out value))
				return false;

			if (!TryParse (text, ref index, out parameters))
				return false;

			if (index >= text.Length || text[index] != ')')
				return false;

			index++;

			disposition = new ContentDisposition (value);

			foreach (var param in parameters)
				disposition.Parameters.Add (param);

			return true;
		}

		static bool TryParse (string text, ref int index, bool multipart, out ContentType contentType)
		{
			IList<Parameter> parameters;
			string type, subtype;

			contentType = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length)
				return false;

			if (!multipart) {
				if (!TryParse (text, ref index, out type))
					return false;
			} else {
				type = "multipart";
			}

			if (!TryParse (text, ref index, out subtype))
				return false;

			if (!TryParse (text, ref index, out parameters))
				return false;

			contentType = new ContentType (type, subtype);

			foreach (var param in parameters)
				contentType.Parameters.Add (param);

			return true;
		}

		static bool TryParse (string text, ref int index, out IList<BodyPart> children)
		{
			BodyPart part;

			children = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length)
				return false;

			if (text[index] != '(') {
				if (index + 3 <= text.Length && text.Substring (index, 3) == "NIL") {
					index += 3;
					return true;
				}

				return false;
			}

			index++;

			if (index >= text.Length)
				return false;

			children = new List<BodyPart> ();

			do {
				if (text[index] == ')')
					break;

				if (!TryParse (text, ref index, out part))
					return false;

				children.Add (part);
			} while (index < text.Length);

			if (index >= text.Length || text[index] != ')')
				return false;

			index++;

			return true;
		}

		static bool TryParse (string text, ref int index, out BodyPart part)
		{
			ContentDisposition disposition;
			ContentType contentType;
			string[] array;
			string nstring;
			uint number;

			part = null;

			while (index < text.Length && text[index] == ' ')
				index++;

			if (index >= text.Length || text[index] != '(')
				return false;

			index++;

			if (index >= text.Length)
				return false;

			if (text[index] == '(') {
				var multipart = new BodyPartMultipart ();
				IList<BodyPart> children;

				index++;

				if (!TryParse (text, ref index, out children))
					return false;

				foreach (var child in children)
					multipart.BodyParts.Add (child);

				if (!TryParse (text, ref index, true, out contentType))
					return false;

				multipart.ContentType = contentType;

				if (!TryParse (text, ref index, out disposition))
					return false;

				multipart.ContentDisposition = disposition;

				if (!TryParse (text, ref index, out array))
					return false;

				multipart.ContentLanguage = array;

				if (!TryParse (text, ref index, out nstring))
					return false;

				multipart.ContentLocation = nstring;

				part = multipart;
			} else {
				BodyPartMessage message = null;
				BodyPartText txt = null;
				BodyPartBasic basic;

				if (!TryParse (text, ref index, false, out contentType))
					return false;

				if (contentType.Matches ("message", "rfc822"))
					basic = message = new BodyPartMessage ();
				else if (contentType.Matches ("text", "*"))
					basic = txt = new BodyPartText ();
				else
					basic = new BodyPartBasic ();

				basic.ContentType = contentType;

				if (!TryParse (text, ref index, out nstring))
					return false;

				basic.ContentId = nstring;

				if (!TryParse (text, ref index, out nstring))
					return false;

				basic.ContentDescription = nstring;

				if (!TryParse (text, ref index, out nstring))
					return false;

				basic.ContentTransferEncoding = nstring;

				if (!TryParse (text, ref index, out number))
					return false;

				basic.Octets = number;

				if (!TryParse (text, ref index, out nstring))
					return false;

				basic.ContentMd5 = nstring;

				if (!TryParse (text, ref index, out disposition))
					return false;

				basic.ContentDisposition = disposition;

				if (!TryParse (text, ref index, out array))
					return false;

				basic.ContentLanguage = array;

				if (!TryParse (text, ref index, out nstring))
					return false;

				basic.ContentLocation = nstring;

				if (message != null) {
					Envelope envelope;
					BodyPart body;

					if (!Envelope.TryParse (text, ref index, out envelope))
						return false;

					message.Envelope = envelope;

					if (!TryParse (text, ref index, out body))
						return false;

					message.Body = body;

					if (!TryParse (text, ref index, out number))
						return false;

					message.Lines = number;
				} else if (txt != null) {
					if (!TryParse (text, ref index, out number))
						return false;

					txt.Lines = number;
				}

				part = basic;
			}

			if (index >= text.Length || text[index] != ')')
				return false;

			index++;

			return true;
		}

		/// <summary>
		/// Tries to parse the given text into a new <see cref="MailKit.BodyPart"/> instance.
		/// </summary>
		/// <remarks>
		/// Parses a body part from the specified text.
		/// </remarks>
		/// <returns><c>true</c>, if the body part was successfully parsed, <c>false</c> otherwise.</returns>
		/// <param name="text">The text to parse.</param>
		/// <param name="part">The parsed body part.</param>
		/// <exception cref="System.ArgumentNullException">
		/// <paramref name="text"/> is <c>null</c>.
		/// </exception>
		public static bool TryParse (string text, out BodyPart part)
		{
			if (text == null)
				throw new ArgumentNullException ("text");

			int index = 0;

			return TryParse (text, ref index, out part) && index == text.Length;
		}
	}
}