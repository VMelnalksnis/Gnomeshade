using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace Tracking.Finance.Interfaces.WindowsDesktop.Input
{
	/// <summary>
	/// Class used to define a multi-key gesture.
	/// </summary>
	[TypeConverter(typeof(MultiKeyGestureConverter))]
	public class MultiKeyGesture : InputGesture
	{
		/// <summary>
		/// The maximum delay between key presses.
		/// </summary>
		private static readonly TimeSpan maximumDelay = TimeSpan.FromSeconds(1);

		/// <summary>
		/// The index of the current gesture key.
		/// </summary>
		private int currentKeyIndex;

		/// <summary>
		/// The current sequence index.
		/// </summary>
		private int currentSequenceIndex;

		/// <summary>
		/// The last time a gesture key was pressed.
		/// </summary>
		private DateTime lastKeyPress;

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiKeyGesture" /> class.
		/// </summary>
		///
		/// <param name="sequences">The key sequences.</param>
		public MultiKeyGesture(params KeySequence[] sequences)
			: this(GetKeySequencesString(sequences), sequences)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MultiKeyGesture" /> class.
		/// </summary>
		///
		/// <param name="displayString">The display string.</param>
		/// <param name="sequences"> he key sequences.</param>
		public MultiKeyGesture(string displayString, params KeySequence[] sequences)
		{
			if (sequences is null)
			{
				throw new ArgumentNullException(nameof(sequences));
			}

			if (sequences.Length == 0)
			{
				throw new ArgumentException("At least one sequence must be specified.", nameof(sequences));
			}

			DisplayString = displayString;
			KeySequences = new KeySequence[sequences.Length];
			sequences.CopyTo(KeySequences, 0);
		}

		/// <summary>
		/// Gets the key sequences composing the gesture.
		/// </summary>
		public KeySequence[] KeySequences { get; }

		/// <summary>
		/// Gets the display string.
		/// </summary>
		public string DisplayString { get; }

		/// <summary>
		/// Determines whether this <see cref="KeyGesture" /> matches the input associated with the specified
		/// <see cref="InputEventArgs" /> object.
		/// </summary>
		///
		/// <param name="targetElement">The target.</param>
		/// <param name="inputEventArgs">The input event data to compare this gesture to.</param>
		/// <returns><see langword="true"/> if the event data matches this <see cref="KeyGesture" />;
		/// otherwise, <see langword="false"/>.</returns>
		public sealed override bool Matches(object targetElement, InputEventArgs inputEventArgs)
		{
			if (inputEventArgs is not KeyEventArgs keyEventArgs || keyEventArgs.IsRepeat)
			{
				return false;
			}

			var key = keyEventArgs.Key != Key.System ? keyEventArgs.Key : keyEventArgs.SystemKey;

			// Check if the key identifies a gesture key...
			if (!IsDefinedKey(key))
			{
				return false;
			}

			var currentSequence = KeySequences[currentSequenceIndex];
			var currentKey = currentSequence.Keys[currentKeyIndex];

			// Check if the key is a modifier...
			if (IsModifierKey(key))
			{
				// If the pressed key is a modifier, ignore it for now, since it is tested afterwards...
				return false;
			}

			// Check if the current key press happened too late...
			if (currentSequenceIndex != 0 && DateTime.Now - lastKeyPress > maximumDelay)
			{
				// The delay has expired, abort the match...
				ResetState();
#if DEBUG_MESSAGES
      System.Diagnostics.Debug.WriteLine("Maximum delay has elapsed", "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
#endif
				return false;
			}

			// Check if current modifiers match required ones...
			if (currentSequence.Modifiers != keyEventArgs.KeyboardDevice.Modifiers)
			{
				// The modifiers are not the expected ones, abort the match...
				ResetState();
#if DEBUG_MESSAGES
      System.Diagnostics.Debug.WriteLine("Incorrect modifier " + args.KeyboardDevice.Modifiers + ", expecting " + currentSequence.Modifiers, "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
#endif
				return false;
			}

			// Check if the current key is not correct...
			if (currentKey != key)
			{
				// The current key is not correct, abort the match...
				ResetState();
#if DEBUG_MESSAGES
      System.Diagnostics.Debug.WriteLine("Incorrect key " + key + ", expecting " + currentKey, "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
#endif
				return false;
			}

			// Move on the index, pointing to the next key...
			currentKeyIndex++;

			// Check if the key is the last of the current sequence...
			if (currentKeyIndex == KeySequences[currentSequenceIndex].Keys.Length)
			{
				// The key is the last of the current sequence, go to the next sequence...
				currentSequenceIndex++;
				currentKeyIndex = 0;
			}

			// Check if the sequence is the last one of the gesture...
			if (currentSequenceIndex != KeySequences.Length)
			{
				// If the key is not the last one, get the current date time, handle the match event but do nothing...
				lastKeyPress = DateTime.Now;
				inputEventArgs.Handled = true;
#if DEBUG_MESSAGES
      System.Diagnostics.Debug.WriteLine("Waiting for " + (m_KeySequences.Length - m_CurrentSequenceIndex) + " sequences", "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
#endif
				return false;
			}

			// The gesture has finished and was correct, complete the match operation...
			ResetState();
			inputEventArgs.Handled = true;
#if DEBUG_MESSAGES
    System.Diagnostics.Debug.WriteLine("Gesture completed " + MultiKeyGestureConverter.Default.ConvertToString(this), "[" + MultiKeyGestureConverter.Default.ConvertToString(this) + "]");
#endif
			return true;
		}

		/// <summary>
		/// Determines whether the keyis define.
		/// </summary>
		///
		/// <param name="key">The key to check.</param>
		/// <returns><see langword="true"/> if the key is defined as a gesture key;
		/// otherwise, <see langword="false"/>.</returns>
		private static bool IsDefinedKey(Key key) => key is >= Key.None and <= Key.OemClear;

		/// <summary>
		/// Gets the key sequences string.
		/// </summary>
		///
		/// <param name="sequences">The key sequences.</param>
		/// <returns>The string representing the key sequences.</returns>
		private static string GetKeySequencesString(params KeySequence[] sequences)
		{
			if (sequences is null)
			{
				throw new ArgumentNullException(nameof(sequences));
			}

			if (sequences.Length == 0)
			{
				throw new ArgumentException("At least one sequence must be specified.", nameof(sequences));
			}

			var builder = new StringBuilder();

			_ = builder.Append(sequences[0].ToString());

			for (var i = 1; i < sequences.Length; i++)
			{
				_ = builder.Append(", " + sequences[i]);
			}

			return builder.ToString();
		}

		/// <summary>
		/// Determines whether the specified key is a modifier key.
		/// </summary>
		///
		/// <param name="key">The key.</param>
		/// <returns><see langword="true"/> if the specified key is a modifier key;
		/// otherwise, <see langword="false"/>.</returns>
		private static bool IsModifierKey(Key key)
		{
			return key is
				Key.LeftCtrl or
				Key.RightCtrl or
				Key.LeftShift or
				Key.RightShift or
				Key.LeftAlt or
				Key.RightAlt or
				Key.LWin or
				Key.RWin;
		}

		/// <summary>
		/// Resets the state of the gesture.
		/// </summary>
		private void ResetState()
		{
			currentSequenceIndex = 0;
			currentKeyIndex = 0;
		}
	}
}
